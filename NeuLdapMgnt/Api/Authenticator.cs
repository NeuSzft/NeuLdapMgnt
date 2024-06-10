using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using NeuLdapMgnt.Api.LdapServiceExtensions;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api;

/// <summary>Stores the result of an authentication attempt.</summary>
/// <param name="Code">The status code to set for the response.</param>
/// <param name="Message">The message to send in the response body.</param>
/// <param name="Username">The username of the authenticated user which should be <c>null</c> on a failed attempt.</param>
public record AuthResult(int Code, string? Message, string? Username) {
	/// <summary>Turns the <see cref="AuthResult"/> into a HTTP result containing the Message.</summary>
	/// <returns>An <see cref="IResult"/> with the text/plain content type and the status code set to Code.</returns>
	public IResult ToResult() {
		return Results.Text(Message, "text/plain", Encoding.UTF8, Code);
	}
}

public static class Authenticator {
	/// <summary>The name of value used to store the default admin enabled within the LDAP database.</summary>
	public const string DefaultAdminEnabledValueName = "default-admin-enabled";

	/// <summary>The name of value used to store the default admin password within the LDAP database.</summary>
	public const string DefaultAdminPasswordValueName = "default-admin-password";

	/// <summary>Returns the name of the default admin.</summary>
	public static string GetDefaultAdminName() => Environment.GetEnvironmentVariable("DEFAULT_ADMIN_NAME").DefaultIfNullOrEmpty("admin");

	/// <summary>Tires to get the default admin password. If it is not or incorrectly set, then sets it to the default value defined by the <c>DEFAULT_ADMIN_PASSWORD</c> environment variable or "<c>adminpass</c>".</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="error">When the method returns, this will contain the error message if there was one. Otherwise it will be set to <c>null</c>.</param>
	/// <returns>The base64 encoded password hash or <c>null</c> if an error occured.</returns>
	/// <remarks>When the default password is set, the default admin is also enabled.</remarks>
	public static string GetDefaultAdminPasswordAndCrateAdminWhenMissing(LdapService ldap, out string? error) {
		string? pwd = ldap.GetValue(DefaultAdminPasswordValueName, out error);
		if (pwd is not null)
			return pwd;

		string bcryptHash = Utils.BCryptHashPassword(Environment.GetEnvironmentVariable("DEFAULT_ADMIN_PASSWORD").DefaultIfNullOrEmpty("adminpass"));
		ldap.SetValue(DefaultAdminEnabledValueName, true.ToString(), out error);
		ldap.SetValue(DefaultAdminPasswordValueName, bcryptHash, out error);

		return bcryptHash;
	}

	/// <summary>Checks whether the default admin is enabled or not.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="error">When the method returns, this will contain the error message if there was one. Otherwise it will be set to <c>null</c>.</param>
	/// <returns><c>true</c> if the value belonging to the <c>DefaultAdminEnabledValueName</c> key is also <c>true</c>, otherwise <c>false</c>.</returns>
	public static bool IsDefaultAdminEnabled(LdapService ldap, out string? error) {
		try {
			return bool.TryParse(ldap.GetValue(DefaultAdminEnabledValueName, out error), out var value) && value;
		}
		catch (Exception e) {
			error = e.ToString();
			return false;
		}
	}

	/// <summary>Tires to get username and password from the Authorization header.</summary>
	/// <param name="request">The <see cref="HttpRequest"/> that contains the Authorization header.</param>
	/// <param name="username">When the method returns, this will contain the username or an empty string on failure.</param>
	/// <param name="password">When the method returns, this will contain the password or an empty string on failure.</param>
	/// <returns><c>true</c> if the username and password were successfully set based on the Authorization header, otherwise <c>false</c>.</returns>
	public static bool TryGetCredentialsFromRequest(HttpRequest request, out string username, out string password) {
		try {
			string   value       = request.Headers.Authorization.ToString().Split(' ')[1];
			string[] credentials = Encoding.UTF8.GetString(Convert.FromBase64String(value)).Split(':');
			username = credentials[0];
			password = credentials[1];
			return true;
		}
		catch {
			username = password = string.Empty;
			return false;
		}
	}

	/// <summary>Tires to authenticate the user specified by the HTTP request via the Basic authentication scheme.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="request">The <see cref="HttpRequest"/> that contains an Authorization header.</param>
	/// <returns>An <see cref="AuthResult"/> containing the result of the authentication attempt.</returns>
	/// <remarks>The authentication currently succeeds regardless of what username is specified as long as the password is "password".</remarks>
	public static AuthResult BasicAuth(LdapService ldap, HttpRequest request) {
		if (request.Headers.Authorization.Count == 0)
			return new(StatusCodes.Status400BadRequest, "Missing Authorization header.", null);

		if (!TryGetCredentialsFromRequest(request, out var username, out var password))
			return new(StatusCodes.Status400BadRequest, "Invalid Authorization header.", null);

		if (username == GetDefaultAdminName()) {
			string bcryptHash = GetDefaultAdminPasswordAndCrateAdminWhenMissing(ldap, out var error);
			if (error is not null)
				return new(StatusCodes.Status503ServiceUnavailable, error, null);

			if (!IsDefaultAdminEnabled(ldap, out error))
				return new(StatusCodes.Status403Forbidden, error ?? "The default admin is not enabled.", null);

			if (Utils.CheckBCryptPassword(bcryptHash, password))
				return new(StatusCodes.Status200OK, null, username);
		}
		else if (ldap.PartOfGroup("inactive", username)) {
			return new(StatusCodes.Status403Forbidden, "User is inactive.", null);
		}
		else if (ldap.PartOfGroup("admin", username) && ldap.TryGetPasswordOfEntity(username, typeof(Employee)) is { } bcryptHash && Utils.CheckBCryptPassword(bcryptHash, password)) {
			return new(StatusCodes.Status200OK, null, username);
		}

		return new(StatusCodes.Status401Unauthorized, "Wrong credentials.", null);
	}

	/// <summary>Reads the JSON Web Token out from the Authorization header of a <see cref="HttpRequest"/>.</summary>
	/// <param name="request">The <see cref="HttpRequest"/> that contains the token within it's Authorization header.</param>
	/// <returns>The <see cref="JwtSecurityToken"/> that was read from the header.</returns>
	/// <exception cref="FormatException">The Authorization header is in an incorrect format.</exception>
	/// <exception cref="SecurityTokenMalformedException">The token is in an incorrect format.</exception>
	public static JwtSecurityToken ReadJwtFromRequestHeader(HttpRequest request) {
		AuthenticationHeaderValue header = AuthenticationHeaderValue.Parse(request.Headers.Authorization.ToString());
		return new JwtSecurityTokenHandler().ReadJwtToken(header.Parameter!);
	}

	/// <summary>Creates a new JSON Web Token for the specified user that will expire in 10 minutes after it's creation.</summary>
	/// <param name="username">The username that will be used as the audience of the token.</param>
	/// <returns>The base64url encoded JSON Web Token.</returns>
	public static string CreateToken(string username) {
		JwtSecurityToken jwt = new(
			Program.ServiceName,
			username,
			null,
			DateTime.Now,
			DateTime.Now.AddMinutes(10),
			new SigningCredentials(Program.SecurityKey, SecurityAlgorithms.HmacSha512)
		);

		return new JwtSecurityTokenHandler().WriteToken(jwt);
	}

	/// <summary>Creates a new JSON Web Token that will use the first specified audience of the previous one. This token also expires in 10 minutes after it's creation.</summary>
	/// <param name="token">The <see cref="JwtSecurityToken"/> that needs to be renewed.</param>
	/// <returns>The base64url encoded JSON Web Token.</returns>
	public static string RenewToken(JwtSecurityToken token) {
		return CreateToken(token.Audiences.First());
	}

	/// <summary>Creates a new JSON Web Token that will use the first specified audience of the previous one. This token also expires in 10 minutes after it's creation.</summary>
	/// <param name="request">The <see cref="HttpRequest"/> that contains the token within it's Authorization header that needs to be renewed.</param>
	/// <returns>The base64url encoded JSON Web Token.</returns>
	/// <exception cref="FormatException">The Authorization header is in an incorrect format.</exception>
	/// <exception cref="SecurityTokenMalformedException">The token is in an incorrect format.</exception>
	public static string RenewToken(HttpRequest request) {
		return RenewToken(ReadJwtFromRequestHeader(request));
	}
}

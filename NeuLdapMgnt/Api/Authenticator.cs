using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using NeuLdapMgnt.Api.LdapServiceExtensions;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api;

/// <summary>Stores the result of an authentication attempt.</summary>
public class AuthResult : IResult {
	private readonly int    _code;
	private readonly string _content;

	/// <param name="username">The username of the authenticated user.</param>
	/// <remarks>The status code will be set to 200 OK.</remarks>
	public AuthResult(string username) {
		_code    = StatusCodes.Status200OK;
		_content = Authenticator.CreateToken(username);
	}

	/// <param name="message">The message to send in the response body.</param>
	/// <param name="code">The status code to set for the response.</param>
	public AuthResult(string message, int code) {
		_code    = code;
		_content = message;
	}

	public async Task ExecuteAsync(HttpContext httpContext) {
		httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(_content);
		httpContext.Response.ContentType   = MediaTypeNames.Text.Plain;
		httpContext.Response.StatusCode    = _code;
		await httpContext.Response.WriteAsync(_content);
	}
}

public static class Authenticator {
	/// <summary>The name of value used to store the default admin enabled within the LDAP database.</summary>
	public const string DefaultAdminEnabledValueName = "default-admin-enabled";

	/// <summary>The name of value used to store the default admin password within the LDAP database.</summary>
	public const string DefaultAdminPasswordValueName = "default-admin-password";

	/// <summary>Returns the name of the default admin.</summary>
	/// <returns>The name specified by the "DEFAULT_ADMIN_NAME" environment variable or "admin" if it is not specified.</returns>
	public static string GetDefaultAdminName() {
		return Utils.GetEnv("DEFAULT_ADMIN_NAME", "admin");
	}

	/// <summary>Tires to authenticate the user specified by the HTTP request via the Basic authentication scheme.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="request">The <see cref="HttpRequest"/> that contains an Authorization header.</param>
	/// <returns>An <see cref="AuthResult"/> containing the result of the authentication attempt.</returns>
	/// <remarks>The authentication currently succeeds regardless of what username is specified as long as the password is "password".</remarks>
	public static AuthResult BasicAuth(LdapService ldap, HttpRequest request) {
		if (request.Headers.Authorization.Count == 0)
			return new("Missing Authorization header.", StatusCodes.Status400BadRequest);

		if (!TryGetCredentialsFromRequest(request, out string username, out string password))
			return new("Invalid Authorization header.", StatusCodes.Status400BadRequest);

		if (username == GetDefaultAdminName()) {
			string bcryptHash = GetDefaultAdminPasswordAndCrateAdminWhenMissing(ldap, out string? error);
			if (error is not null)
				return new(error, StatusCodes.Status503ServiceUnavailable);

			if (!IsDefaultAdminEnabled(ldap, out error))
				return new(error ?? "The default admin is not enabled.", StatusCodes.Status403Forbidden);

			if (Utils.CheckBCryptPassword(bcryptHash, password))
				return new(username);
		}
		else if (ldap.TryGetEntity<Employee>(username, true) is { Value: { } employee }) {
			if (employee.IsInactive)
				return new("User is inactive.", StatusCodes.Status403Forbidden);

			if (employee is { IsAdmin: true } && employee.CheckPassword(password))
				return new(username);
		}

		return new("Wrong credentials.", StatusCodes.Status401Unauthorized);
	}

	/// <summary>Reads the JSON Web Token out from the Authorization header of a <see cref="HttpRequest"/>.</summary>
	/// <param name="request">The <see cref="HttpRequest"/> that contains the token within its Authorization header.</param>
	/// <returns>The <see cref="JwtSecurityToken"/> that was read from the header.</returns>
	/// <exception cref="FormatException">The Authorization header is in an incorrect format.</exception>
	/// <exception cref="SecurityTokenMalformedException">The token is in an incorrect format.</exception>
	public static JwtSecurityToken ReadJwtFromRequestHeader(HttpRequest request) {
		AuthenticationHeaderValue header = AuthenticationHeaderValue.Parse(request.Headers.Authorization.ToString());
		return new JwtSecurityTokenHandler().ReadJwtToken(header.Parameter!);
	}

	/// <summary>Creates a new JSON Web Token for the specified user that will expire in 10 minutes after its creation.</summary>
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

	/// <summary>Creates a new JSON Web Token that will use the first specified audience of the previous one. This token also expires in 10 minutes after its creation.</summary>
	/// <param name="request">The <see cref="HttpRequest"/> that contains the token within its Authorization header that needs to be renewed.</param>
	/// <returns>The base64url encoded JSON Web Token.</returns>
	/// <exception cref="FormatException">The Authorization header is in an incorrect format.</exception>
	/// <exception cref="SecurityTokenMalformedException">The token is in an incorrect format.</exception>
	public static string RenewToken(HttpRequest request) {
		return CreateToken(ReadJwtFromRequestHeader(request).Audiences.First());
	}

	/// <summary>Tires to get the default admin password. If it is not or incorrectly set, then sets it to the default value defined by the <c>DEFAULT_ADMIN_PASSWORD</c> environment variable or "<c>adminpass</c>".</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="error">When the method returns, this will contain the error message if there was one. Otherwise, it will be set to <c>null</c>.</param>
	/// <returns>The base64 encoded password hash or <c>null</c> if an error occured.</returns>
	/// <remarks>When the default password is set, the default admin is also enabled.</remarks>
	private static string GetDefaultAdminPasswordAndCrateAdminWhenMissing(LdapService ldap, out string? error) {
		string? pwd = ldap.GetValue(DefaultAdminPasswordValueName, out error);
		if (pwd is not null)
			return pwd;

		string bcryptHash = Utils.BCryptHashPassword(Utils.GetEnv("DEFAULT_ADMIN_PASSWORD", "adminpass"));
		ldap.SetValue(DefaultAdminEnabledValueName, true.ToString(), out error);
		ldap.SetValue(DefaultAdminPasswordValueName, bcryptHash, out error);

		return bcryptHash;
	}

	/// <summary>Checks whether the default admin is enabled or not.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="error">When the method returns, this will contain the error message if there was one. Otherwise, it will be set to <c>null</c>.</param>
	/// <returns><c>true</c> if the value belonging to the <c>DefaultAdminEnabledValueName</c> key is also <c>true</c>, otherwise <c>false</c>.</returns>
	private static bool IsDefaultAdminEnabled(LdapService ldap, out string? error) {
		try {
			return bool.TryParse(ldap.GetValue(DefaultAdminEnabledValueName, out error), out bool value) && value;
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
	private static bool TryGetCredentialsFromRequest(HttpRequest request, out string username, out string password) {
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
}

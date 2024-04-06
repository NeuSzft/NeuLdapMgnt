using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace NeuLdapMgnt.Api;

/// <summary>Stores the result of an authentication attempt.</summary>
public record AuthResult(int Code, string? Message, string? Username) {
    /// <summary>Turns the <see cref="AuthResult"/> into a HTTP result containing the Message.</summary>
    /// <returns>An <see cref="IResult"/> with the text/plain content type and the status code set to Code.</returns>
    public virtual IResult ToResult() {
        return Results.Text(Message, "text/plain", Encoding.UTF8, Code);
    }
}

public static class AuthHelper {
    // TODO: Authenticate via LDAP database
    /// <summary>Tires to authenticate the user specified by the HTTP request via the Basic authentication scheme.</summary>
    /// <param name="request">The <see cref="HttpRequest"/> that contains an Authorization header.</param>
    /// <returns>An <see cref="AuthResult"/> containing the result of the authentication attempt.</returns>
    /// <remarks>The authentication currently succeeds regardless of what username is specified as long as the password is "password".</remarks>
    public static AuthResult BasicAuth(HttpRequest request) {
        string username, password;

        if (request.Headers.Authorization.Count == 0)
            return new(StatusCodes.Status400BadRequest, "Missing Authorization header.", null);

        try {
            string   value       = request.Headers.Authorization.ToString().Split(' ')[1];
            string[] credentials = Encoding.UTF8.GetString(Convert.FromBase64String(value)).Split(':');
            username = credentials[0];
            password = credentials[1];
        }
        catch {
            return new(StatusCodes.Status400BadRequest, "Invalid Authorization header.", null);
        }

        byte[] hash = SHA512.HashData(Encoding.UTF8.GetBytes(password));
        if (!hash.SequenceEqual(Base64UrlEncoder.DecodeBytes("sQnzu7wkTrgkQZF-0G1hi5AI3Qmzvv0bXgc5THBqi7mAsdd4Xll27ASbRt9fEyavWi6m0QP9B8lThf-rDKy8hg")))
            return new(StatusCodes.Status401Unauthorized, "Wrong credentials.", null);

        return new(StatusCodes.Status200OK, null, username);
    }

    /// <summary>Creates a new JSON Web Token for the specified user that will expire in 10 minutes after it's creation.</summary>
    /// <param name="username">The username that will be used as the audience of the token.</param>
    /// <returns>The base64url encoded JSON Web Token.</returns>
    public static string CreateToken(string username) {
        JwtSecurityToken jwt = new(
            Program.TokenIssuer,
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
    public static string RenewToken(HttpRequest request) {
        AuthenticationHeaderValue header = AuthenticationHeaderValue.Parse(request.Headers.Authorization.ToString());
        JwtSecurityToken          token  = new JwtSecurityTokenHandler().ReadJwtToken(header.Parameter!);
        return RenewToken(token);
    }
}

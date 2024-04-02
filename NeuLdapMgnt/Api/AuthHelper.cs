using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace NeuLdapMgnt.Api;

public record AuthResult(int Code, string? Message, string? Username) {
    public virtual IResult ToResult() {
        return Results.Text(Message, "text/plain", Encoding.UTF8, Code);
    }
}

public static class AuthHelper {
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

    public static string RenewToken(JwtSecurityToken token) {
        return CreateToken(token.Audiences.First());
    }

    public static string RenewToken(HttpRequest request) {
        AuthenticationHeaderValue header = AuthenticationHeaderValue.Parse(request.Headers.Authorization.ToString());
        JwtSecurityToken          token  = new JwtSecurityTokenHandler().ReadJwtToken(header.Parameter!);
        return RenewToken(token);
    }
}

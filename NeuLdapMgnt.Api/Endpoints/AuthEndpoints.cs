using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace NeuLdapMgnt.Api.Endpoints;

public static class AuthEndpoints {
    public static void MapAuthEndpoints(this WebApplication app) {
        app.MapGet("/auth/login", (HttpRequest request) => {
               string username, password;

               try {
                   string   value       = request.Headers.Authorization.ToString().Split(' ')[1];
                   string[] credentials = Encoding.UTF8.GetString(Convert.FromBase64String(value)).Split(':');
                   username = credentials[0];
                   password = credentials[1];
               }
               catch {
                   return Results.BadRequest("Invalid Authorization header.");
               }

               byte[] hash = SHA512.HashData(Encoding.UTF8.GetBytes(password));
               if (!hash.SequenceEqual(Base64UrlEncoder.DecodeBytes("sQnzu7wkTrgkQZF-0G1hi5AI3Qmzvv0bXgc5THBqi7mAsdd4Xll27ASbRt9fEyavWi6m0QP9B8lThf-rDKy8hg")))
                   return Results.Unauthorized();

               JwtSecurityToken jwt = new(
                   Program.TokenIssuer,
                   username,
                   null,
                   DateTime.Now,
                   DateTime.Now.AddMinutes(5),
                   new SigningCredentials(Program.SecurityKey, SecurityAlgorithms.HmacSha512)
               );

               return Results.Text(new JwtSecurityTokenHandler().WriteToken(jwt));
           })
           .WithOpenApi()
           .WithName("BasicAuth")
           .WithTags("Authentication")
           .Produces<string>()
           .Produces<string>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status401Unauthorized);
    }
}

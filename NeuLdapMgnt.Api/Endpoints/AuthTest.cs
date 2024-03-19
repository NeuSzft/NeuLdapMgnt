using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace NeuLdapMgnt.Api.Endpoints;

internal static class Endpoints {
    public static void MapAuthTestEndpoints(this WebApplication app) {
        app.MapGet("/auth/test", () => Results.Ok()).RequireAuthorization();

        app.MapGet("/auth/token", (HttpContext context) => {
            JwtSecurityToken jwt = new(
                Program.TokenIssuer,
                "TestUser",
                null,
                DateTime.Now,
                DateTime.Now.AddHours(2),
                new SigningCredentials(Program.SecurityKey, SecurityAlgorithms.HmacSha256)
            );

            string token = new JwtSecurityTokenHandler().WriteToken(jwt);

            context.Response.Cookies.Append("jwt", token);
            return Results.Text(token);
        });
    }
}

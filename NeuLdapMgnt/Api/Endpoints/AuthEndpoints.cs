using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class AuthEndpoints {
    public static void MapAuthEndpoints(this WebApplication app) {
        app.MapGet("/api/auth", (LdapService ldap, HttpRequest request) => {
               var result = Authenticator.BasicAuth(ldap, request);
               return result.Username is null ? result.ToResult() : Results.Text(Authenticator.CreateToken(result.Username));
           })
           .WithOpenApi()
           .WithName("BasicAuth")
           .WithTags("Authentication")
           .Produces<string>()
           .Produces<string>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status401Unauthorized)
           .Produces<string>(StatusCodes.Status403Forbidden)
           .Produces<string>(StatusCodes.Status500InternalServerError)
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
    }
}

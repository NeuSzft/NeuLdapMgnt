using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

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
           .WithDescription("### Performs Basic authentication and returns a Json Web Token on successful authentication.")
           .Produces<string>()
           .Produces<string>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status401Unauthorized)
           .Produces<string>(StatusCodes.Status403Forbidden)
           .Produces<string>(StatusCodes.Status500InternalServerError)
           .Produces<string>(StatusCodes.Status503ServiceUnavailable);
    }
}

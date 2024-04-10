using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace NeuLdapMgnt.Api.Endpoints;

public static class AuthEndpoints {
    public static void MapAuthEndpoints(this WebApplication app) {
        app.MapGet("/api/auth", (HttpRequest request) => {
               var result = Authenticator.BasicAuth(request);
               return result.Username is null ? result.ToResult() : Results.Text(Authenticator.CreateToken(result.Username));
           })
           .WithOpenApi()
           .WithName("BasicAuth")
           .WithTags("Authentication")
           .Produces<string>()
           .Produces<string>(StatusCodes.Status400BadRequest)
           .Produces(StatusCodes.Status401Unauthorized);
    }
}

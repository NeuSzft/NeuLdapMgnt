using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace NeuLdapMgnt.Api.Endpoints;

public static class AuthEndpoints {
    public static void MapAuthEndpoints(this WebApplication app) {
        app.MapGet("/auth", (HttpRequest request) => {
               var result = AuthHelper.BasicAuth(request);
               return result.Username is null ? result.ToResult() : Results.Text(AuthHelper.CreateToken(result.Username));
           })
           .WithOpenApi()
           .WithName("BasicAuth")
           .WithTags("Authentication")
           .Produces<string>()
           .Produces<string>(StatusCodes.Status400BadRequest)
           .Produces(StatusCodes.Status401Unauthorized);
    }
}

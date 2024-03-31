using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace NeuLdapMgnt.Api.Endpoints;

public static class TestingEndpoints {
    public static void MapTestingEndpoints(this WebApplication app) {
        app.MapGet("/testing/auth", (HttpRequest request) =>
               Results.Text(AuthHelper.CreateToken("testuser"))
           )
           .WithOpenApi()
           .WithTags("Testing")
           .Produces<string>();
    }
}

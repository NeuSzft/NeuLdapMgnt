using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class DbManagementEndpoints {
    public static void MapDbManagementEndpoints(this WebApplication app) {
        app.MapPost("/api/database/import", async (LdapService ldap, HttpRequest request) => {
               var result = await ModelValidator.ValidateRequest<LdapDbDump>(request);
               if (result.IsFailure())
                   return result.RenewToken(request).ToResult();

               return ldap.ImportDatabase(result.GetValue()!).RenewToken(request).ToResult();
           })
           .WithOpenApi()
           .WithTags("Database Management")
           .RequireAuthorization()
           .Accepts<LdapDbDump>("application/json")
           .Produces<RequestResult>(StatusCodes.Status207MultiStatus)
           .Produces<RequestResult>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

        app.MapGet("/api/database/export", (LdapService ldap, HttpRequest request) =>
               ldap.ExportDatabase().RenewToken(request).ToResult()
           )
           .WithOpenApi()
           .WithTags("Database Management")
           .RequireAuthorization()
           .Produces<RequestResult>(StatusCodes.Status207MultiStatus)
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

        app.MapPut("/api/database/erase-and-import", async (LdapService ldap, HttpRequest request) => {
               var result = await ModelValidator.ValidateRequest<LdapDbDump>(request);
               if (result.IsFailure())
                   return result.RenewToken(request).ToResult();

               return ldap.ImportDatabase(result.GetValue()!, true).RenewToken(request).ToResult();
           })
           .WithOpenApi()
           .WithTags("Database Management")
           .RequireAuthorization()
           .Accepts<LdapDbDump>("application/json")
           .Produces<RequestResult>(StatusCodes.Status207MultiStatus)
           .Produces<RequestResult>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<RequestResult>(StatusCodes.Status500InternalServerError)
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
    }
}

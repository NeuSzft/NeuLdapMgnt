using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class AdminUserEndpoints {
    public static void MapAdminUserEndpoints(this WebApplication app) {
        app.MapGet("/admins", (LdapService ldap, HttpRequest request) => {
               IEnumerable<long> uids = ldap.GetMembersOfGroup("admins");
               return new RequestResult<IEnumerable<long>>().SetValues(uids).RenewToken(request).ToResult();
           })
           .WithOpenApi()
           .WithTags("Admin Users")
           .RequireAuthorization()
           .Produces<RequestResult<long[]>>()
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

        app.MapPost("/admins/{id}", (LdapService ldap, HttpRequest request, long id) => {
               var result = ldap.TryAddEntityToGroup("admins", id);
               return result.RenewToken(request).ToResult();
           })
           .WithOpenApi()
           .WithTags("Admin Users")
           .RequireAuthorization()
           .Produces(StatusCodes.Status200OK)
           .Produces<RequestResult>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<RequestResult>(StatusCodes.Status409Conflict)
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

        app.MapDelete("/admins/{id}", (LdapService ldap, HttpRequest request, long id) => {
               var result = ldap.TryRemoveEntityFromGroup("admins", id);
               return result.RenewToken(request).ToResult();
           })
           .WithOpenApi()
           .WithTags("Admin Users")
           .RequireAuthorization()
           .Produces(StatusCodes.Status200OK)
           .Produces<RequestResult>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
    }
}

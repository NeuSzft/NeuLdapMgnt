using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Api.LdapServiceExtensions;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class InactiveUserEndpoints {
    public static void MapInactiveUserEndpoints(this WebApplication app) {
        app.MapGet("/api/inactives", (LdapService ldap, HttpRequest request) => {
               IEnumerable<string> uids = ldap.GetMembersOfGroup("inactive");
               return new RequestResult<IEnumerable<string>>().SetValues(uids).RenewToken(request).ToResult();
           })
           .WithOpenApi()
           .WithTags("Inactive Users")
           .RequireAuthorization()
           .Produces<RequestResult<string[]>>()
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

        app.MapPost("/api/inactives/{id}", (LdapService ldap, HttpRequest request, string id) => {
               var result = ldap.TryAddEntityToGroup("inactive", id);
               return result.RenewToken(request).ToResult();
           })
           .WithOpenApi()
           .WithTags("Inactive Users")
           .RequireAuthorization()
           .Produces(StatusCodes.Status200OK)
           .Produces<RequestResult>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<RequestResult>(StatusCodes.Status409Conflict)
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

        app.MapDelete("/api/inactives/{id}", (LdapService ldap, HttpRequest request, string id) => {
               var result = ldap.TryRemoveEntityFromGroup("inactive", id);
               return result.RenewToken(request).ToResult();
           })
           .WithOpenApi()
           .WithTags("Inactive Users")
           .RequireAuthorization()
           .Produces(StatusCodes.Status200OK)
           .Produces<RequestResult>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
    }
}

using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class AdminUserEndpoints {
    public static void MapAdminUserEndpoints(this WebApplication app) {
        app.MapGet("/api/admins", (LdapService ldap, HttpRequest request) => {
               IEnumerable<string> uids = ldap.GetMembersOfGroup("admins");
               return new RequestResult<IEnumerable<string>>().SetValues(uids).RenewToken(request).ToResult();
           })
           .WithOpenApi()
           .WithTags("Admin Users")
           .RequireAuthorization()
           .Produces<RequestResult<string[]>>()
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

        app.MapPost("/api/admins/{id}", (LdapService ldap, HttpRequest request, string id) => {
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

        app.MapDelete("/api/admins/{id}", (LdapService ldap, HttpRequest request, string id) => {
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

        app.MapPut("/api/defaultadmin/enable", async (LdapService ldap, HttpRequest request) => {
               using StreamReader reader = new(request.Body);
               string             value  = await reader.ReadToEndAsync();

               if (!bool.TryParse(value, out var enable))
                   return new RequestResult().SetStatus(StatusCodes.Status400BadRequest).SetErrors($"'{value}' is not a valid boolean.").RenewToken(request).ToResult();

               UserPassword? userPassword = Authenticator.GetAndSetDefaultAdminPasswordIfNotSet(ldap, out var error);
               if (error is not null || userPassword is null)
                   return new RequestResult().SetStatus(StatusCodes.Status503ServiceUnavailable).SetErrors(error ?? "Unknown error.").RenewToken(request).ToResult();

               userPassword.Locked = !enable;
               ldap.SetValue(Authenticator.DefaultAdminPasswordValueName, userPassword.ToString(), out error);

               if (error is not null)
                   return new RequestResult().SetStatus(StatusCodes.Status503ServiceUnavailable).SetErrors(error).RenewToken(request).ToResult();
               return new RequestResult().RenewToken(request).ToResult();
           })
           .WithOpenApi()
           .WithTags("Admin Users")
           .RequireAuthorization()
           .Accepts<bool>("text/plain")
           .Produces<RequestResult>()
           .Produces<RequestResult>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

        app.MapPut("/api/defaultadmin/password", async (LdapService ldap, HttpRequest request) => {
               using StreamReader reader   = new(request.Body);
               string             password = await reader.ReadToEndAsync();

               if (password.Length < 8)
                   return new RequestResult().SetStatus(StatusCodes.Status400BadRequest).SetErrors("Password must be at least 8 characters long.").RenewToken(request).ToResult();

               bool locked = ldap.GetValue(Authenticator.DefaultAdminPasswordValueName, out _)?.StartsWith('!') ?? false;
               password = new UserPassword(password, 16, locked).ToString();

               ldap.SetValue(Authenticator.DefaultAdminPasswordValueName, password, out var error);

               if (error is not null)
                   return new RequestResult().SetStatus(StatusCodes.Status503ServiceUnavailable).SetErrors(error).RenewToken(request).ToResult();
               return new RequestResult().RenewToken(request).ToResult();
           })
           .WithOpenApi()
           .WithTags("Admin Users")
           .RequireAuthorization()
           .Accepts<string>("text/plain")
           .Produces<RequestResult>()
           .Produces<RequestResult>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
    }
}

using System;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class ClassEndpoints {
    public static void MapClassEndpoints(this WebApplication app) {
        app.MapGet("/api/classes", (LdapService ldap, HttpRequest request) => {
               string?  value   = ldap.GetValue("classes", out var error);
               string[] classes = value?.Split(';') ?? [];

               RequestResult<string[]> result = new();
               if (error is not null)
                   result.SetErrors(error);

               return result.SetValues(classes).RenewToken(request).ToResult();
           })
           .WithOpenApi()
           .WithTags("Classes")
           .RequireAuthorization()
           .Produces<RequestResult<string[]>>()
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

        app.MapPut("/api/classes", async (LdapService ldap, HttpRequest request) => {
               string[]? classes;
               try {
                   classes = await JsonSerializer.DeserializeAsync<string[]>(request.Body);
               }
               catch (Exception e) {
                   return new RequestResult().SetStatus(StatusCodes.Status400BadRequest).SetErrors(e.GetError()).RenewToken(request).ToResult();
               }

               bool success = ldap.SetValue("classes", string.Join(';', classes ?? []), out var error);

               RequestResult<string[]> result = new();
               if (error is not null)
                   result.SetErrors(error);

               return result.SetStatus(success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest).RenewToken(request).ToResult();
           })
           .WithOpenApi()
           .WithTags("Classes")
           .RequireAuthorization()
           .Accepts<string[]>("application/json")
           .Produces<RequestResult>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
    }
}

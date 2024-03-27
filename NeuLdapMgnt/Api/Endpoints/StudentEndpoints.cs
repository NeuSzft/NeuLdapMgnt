using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class StudentEndpoints {
    public static void MapStudentEndpoints(this WebApplication app) {
        app.MapGet("/students", (LdapHelper ldapHelper) => {
               IEnumerable<Student> students = ldapHelper.GetAllEntities<Student>();
               return Results.Ok(students);
           })
           .WithOpenApi()
           .WithTags("Students")
           .RequireAuthorization()
           .Produces<IEnumerable<Student>>()
           .Produces<string>(StatusCodes.Status401Unauthorized)
           .Produces<string>(StatusCodes.Status503ServiceUnavailable, "text/plain");

        app.MapGet("/students/{id}", (LdapHelper ldapHelper, long id) =>
               ldapHelper.TryGetEntity<Student>(id).ToResult()
           )
           .WithOpenApi()
           .WithTags("Students")
           .RequireAuthorization()
           .Produces<Student>()
           .Produces<string>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status401Unauthorized)
           .Produces<string>(StatusCodes.Status404NotFound)
           .Produces<string>(StatusCodes.Status503ServiceUnavailable, "text/plain");

        app.MapPost("/students", async (LdapHelper ldapHelper, HttpRequest request) => {
               var validation = await ModelValidator.ValidateRequest<Student>(request);
               return validation.Result is null
                   ? validation.ToResult()
                   : ldapHelper.TryAddEntity(validation.Result, validation.Result.Id).ToResult();
           })
           .WithOpenApi()
           .WithTags("Students")
           .RequireAuthorization()
           .Accepts<Student>("application/json")
           .Produces(StatusCodes.Status201Created)
           .Produces<string>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status401Unauthorized)
           .Produces<string>(StatusCodes.Status409Conflict)
           .Produces<string>(StatusCodes.Status503ServiceUnavailable, "text/plain");

        app.MapPut("/students/{id}", async (LdapHelper ldapHelper, HttpRequest request, long id) => {
               var validation = await ModelValidator.ValidateRequest<Student>(request);
               return validation.Result is null
                   ? validation.ToResult()
                   : ldapHelper.TryModifyEntity(validation.Result, id).ToResult();
           })
           .WithOpenApi()
           .WithTags("Students")
           .RequireAuthorization()
           .Accepts<Student>("application/json")
           .Produces(StatusCodes.Status200OK)
           .Produces<string>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status401Unauthorized)
           .Produces<string>(StatusCodes.Status404NotFound)
           .Produces<string>(StatusCodes.Status503ServiceUnavailable, "text/plain");

        app.MapDelete("/students/{id}", (LdapHelper ldapHelper, long id) =>
               ldapHelper.TryDeleteEntity<Student>(id).ToResult()
           )
           .WithOpenApi()
           .WithTags("Students")
           .RequireAuthorization()
           .Produces(StatusCodes.Status200OK)
           .Produces<string>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status401Unauthorized)
           .Produces<string>(StatusCodes.Status404NotFound)
           .Produces<string>(StatusCodes.Status503ServiceUnavailable, "text/plain");
    }
}

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
           .Produces<IEnumerable<Student>>();

        app.MapGet("/students/{id}", (LdapHelper ldapHelper, string id) =>
               ldapHelper.TryGetEntity<Student>(id).ToResult()
           )
           .WithOpenApi()
           .Produces<Student>()
           .Produces<string>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status404NotFound);

        app.MapPost("/students", async (LdapHelper ldapHelper, HttpRequest request) => {
               var validation = await ModelValidator.ValidateRequest<Student>(request);
               return validation.Result is null
                   ? validation.ToResult()
                   : ldapHelper.TryAddEntity(validation.Result, validation.Result.Id).ToResult();
           })
           .WithOpenApi()
           .Accepts<Student>("application/json")
           .Produces(StatusCodes.Status201Created)
           .Produces<string>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status409Conflict);

        app.MapPut("/students/{id}", async (LdapHelper ldapHelper, HttpRequest request, string id) => {
               var validation = await ModelValidator.ValidateRequest<Student>(request);
               return validation.Result is null
                   ? validation.ToResult()
                   : ldapHelper.TryModifyEntity(validation.Result, id).ToResult();
           })
           .WithOpenApi()
           .Accepts<Student>("application/json")
           .Produces(StatusCodes.Status200OK)
           .Produces<string>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status404NotFound);

        app.MapDelete("/students/{id}", (LdapHelper ldapHelper, string id) =>
               ldapHelper.TryDeleteEntity<Student>(id).ToResult()
           )
           .WithOpenApi()
           .Produces(StatusCodes.Status200OK)
           .Produces<string>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status404NotFound);
    }
}

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

        app.MapPost("/students", (LdapHelper ldapHelper, Student student) =>
               ldapHelper.TryAddEntity(student, student.Id).ToResult()
           )
           .WithOpenApi()
           .Accepts<Student>("application/json")
           .Produces(StatusCodes.Status201Created)
           .Produces<string>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status409Conflict);

        app.MapPut("/students/{id}", (LdapHelper ldapHelper, Student student, string id) =>
               ldapHelper.TryModifyEntity(student, id).ToResult()
           )
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

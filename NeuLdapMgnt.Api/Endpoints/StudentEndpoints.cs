using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Api.Connectors;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class StudentEndpoints {
    public static void MapStudentEndpoints(this WebApplication app) {
        app.MapGet("/students", (LdapHelper ldapHelper) => {
               IEnumerable<Student> students = ldapHelper.GetAllStudents();
               return Results.Ok(students);
           })
           .WithOpenApi()
           .Produces<Student>();

        app.MapGet("/students/{id}", (LdapHelper ldapHelper, string id) => {
               if (ldapHelper.TryGetStudent(id, out var error) is { } student)
                   return Results.Ok(student);
               return Results.NotFound(error);
           })
           .WithOpenApi()
           .Produces<Student>()
           .Produces<string>(StatusCodes.Status404NotFound);

        app.MapPost("/students", (LdapHelper ldapHelper, Student student) => {
               bool success = ldapHelper.TryAddStudent(student, student.Id, out var error);
               return success ? Results.Created() : Results.BadRequest(error);
           })
           .WithOpenApi()
           .Accepts<Student>("application/json")
           .Produces(StatusCodes.Status201Created)
           .Produces<string>(StatusCodes.Status400BadRequest);
    }
}

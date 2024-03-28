using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<string>(StatusCodes.Status503ServiceUnavailable, "text/plain");

        app.MapGet("/students/{id}", (LdapHelper ldapHelper, long id) =>
               ldapHelper.TryGetEntity<Student>(id).ToResult()
           )
           .WithOpenApi()
           .WithTags("Students")
           .RequireAuthorization()
           .Produces<Student>()
           .Produces<string>(StatusCodes.Status400BadRequest, "text/plain")
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<string>(StatusCodes.Status404NotFound, "text/plain")
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
           .Produces<string>(StatusCodes.Status400BadRequest, "text/plain")
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<string>(StatusCodes.Status409Conflict, "text/plain")
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
           .Produces<string>(StatusCodes.Status400BadRequest, "text/plain")
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<string>(StatusCodes.Status404NotFound, "text/plain")
           .Produces<string>(StatusCodes.Status503ServiceUnavailable, "text/plain");

        app.MapDelete("/students/{id}", (LdapHelper ldapHelper, long id) =>
               ldapHelper.TryDeleteEntity<Student>(id).ToResult()
           )
           .WithOpenApi()
           .WithTags("Students")
           .RequireAuthorization()
           .Produces(StatusCodes.Status200OK)
           .Produces<string>(StatusCodes.Status400BadRequest, "text/plain")
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<string>(StatusCodes.Status404NotFound, "text/plain")
           .Produces<string>(StatusCodes.Status503ServiceUnavailable, "text/plain");

        app.MapPost("/students/import", async (LdapHelper ldapHelper, HttpRequest request) => {
               using StreamReader reader = new(request.Body);

               var results = await Utils.CsvToStudents(reader);
               if (results.Error is not null)
                   return Results.Text(results.Error, "text/plain", null, StatusCodes.Status400BadRequest);

               return ldapHelper.TryAddEntities(results.Students, student => student.Id).ToResult();
           })
           .WithOpenApi()
           .WithTags("Students")
           .RequireAuthorization()
           .Accepts<IFormFile>("text/csv")
           .Produces<string>(StatusCodes.Status207MultiStatus, "application/json")
           .Produces<string>(StatusCodes.Status400BadRequest, "text/plain")
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<string>(StatusCodes.Status503ServiceUnavailable, "text/plain");

        app.MapGet("/students/export", (LdapHelper ldapHelper, HttpContext context) => {
               IEnumerable<Student> students = ldapHelper.GetAllEntities<Student>();

               StringBuilder sb = new();
               foreach (Student s in students)
                   sb.AppendLine($"{s.Id},{s.FirstName},{s.LastName},{s.Email},{s.Password}");

               return Results.File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"students-{DateTime.Now.Ticks}.csv");
           })
           .WithOpenApi()
           .WithTags("Students")
           .RequireAuthorization()
           .Produces<IFormFile>(StatusCodes.Status200OK, "text/csv")
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<string>(StatusCodes.Status503ServiceUnavailable, "text/plain");
    }
}

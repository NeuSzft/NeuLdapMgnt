using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class StudentEndpoints {
    public static void MapStudentEndpoints(this WebApplication app) {
        app.MapGet("/api/students", (LdapService ldap, HttpRequest request) => {
               Student[] students = ldap.GetAllEntities<Student>().ToArray();
               return new RequestResult<Student>().SetValues(students).RenewToken(request).ToResult();
           })
           .WithOpenApi()
           .WithTags("Students")
           .RequireAuthorization()
           .Produces<RequestResult<Student>>()
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

        app.MapGet("/api/students/{id}", (LdapService ldap, HttpRequest request, string id) => {
               var result = ldap.TryGetEntity<Student>(id);
               return result.RenewToken(request).ToResult();
           })
           .WithOpenApi()
           .WithTags("Students")
           .RequireAuthorization()
           .Produces<Student>()
           .Produces<RequestResult>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<RequestResult>(StatusCodes.Status404NotFound)
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

        app.MapPost("/api/students", async (LdapService ldap, HttpRequest request) => {
               var result = await ModelValidator.ValidateRequest<Student>(request);
               if (result.IsFailure())
                   return result.RenewToken(request).ToResult();
               return ldap.TryAddEntity(result.GetValue()!, result.GetValue()!.Id.ToString()).RenewToken(request).ToResult();
           })
           .WithOpenApi()
           .WithTags("Students")
           .RequireAuthorization()
           .Accepts<Student>("application/json")
           .Produces(StatusCodes.Status201Created)
           .Produces<RequestResult>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<RequestResult>(StatusCodes.Status409Conflict)
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

        app.MapPut("/api/students/{id}", async (LdapService ldap, HttpRequest request, string id) => {
               var result = await ModelValidator.ValidateRequest<Student>(request);
               if (result.IsFailure())
                   return result.RenewToken(request).ToResult();
               return ldap.TryModifyEntity(result.GetValue()!, id).RenewToken(request).ToResult();
           })
           .WithOpenApi()
           .WithTags("Students")
           .RequireAuthorization()
           .Accepts<Student>("application/json")
           .Produces(StatusCodes.Status200OK)
           .Produces<RequestResult>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<RequestResult>(StatusCodes.Status404NotFound)
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

        app.MapDelete("/api/students/{id}", (LdapService ldap, HttpRequest request, string id) =>
               ldap.TryDeleteEntity<Student>(id).RenewToken(request).ToResult()
           )
           .WithOpenApi()
           .WithTags("Students")
           .RequireAuthorization()
           .Produces(StatusCodes.Status200OK)
           .Produces<RequestResult>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<RequestResult>(StatusCodes.Status404NotFound)
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

        app.MapPost("/api/students/import", async (LdapService ldap, HttpRequest request) => {
               using StreamReader reader = new(request.Body);

               var results = await Utils.CsvToStudents(reader);
               if (results.Error is not null)
                   return Results.Text(results.Error, "text/plain", null, StatusCodes.Status400BadRequest);

               return ldap.TryAddEntities(results.Students, student => student.Id.ToString()).RenewToken(request).ToResult();
           })
           .WithOpenApi()
           .WithTags("Students")
           .RequireAuthorization()
           .Accepts<IFormFile>("text/csv")
           .Produces<RequestResult>(StatusCodes.Status207MultiStatus)
           .Produces<RequestResult>(StatusCodes.Status400BadRequest)
           .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

        app.MapGet("/api/students/export", (LdapService ldap) => {
               IEnumerable<Student> students = ldap.GetAllEntities<Student>();

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
           .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
    }
}

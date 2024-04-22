using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Api.LdapServiceExtensions;
using NeuLdapMgnt.Models;
using NeuLdapMgnt.Models.CustomValidationAttributes;

namespace NeuLdapMgnt.Api.Endpoints;

public static class StudentEndpoints {
	public static void MapStudentEndpoints(this WebApplication app) {
		app.MapGet("/api/students", (LdapService ldap, HttpRequest request) =>
			   ldap.GetAllEntities<Student>().RenewToken(request).ToResult()
		   )
		   .WithOpenApi()
		   .WithTags("Students")
		   .RequireAuthorization()
		   .Produces<RequestResult<Student>>(StatusCodes.Status207MultiStatus)
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

		app.MapPut("/api/students/{id}/password", async (LdapService ldap, HttpRequest request, string id) => {
			   using StreamReader reader   = new(request.Body);
			   string             password = await reader.ReadToEndAsync();


			   var validation = ModelValidator.ValidateValue(password, new PasswordAttribute());
			   if (validation.IsFailure())
				   return validation.RenewToken(request).ToResult();

			   var response = ldap.TryGetEntity<Student>(id, true);
			   if (response.IsFailure())
				   return response.RenewToken(request).ToResult();

			   Student student = response.GetValue()!;
			   student.Password = new UserPassword(password, 16).ToString();

			   return ldap.TryModifyEntity(student, id, true).RenewToken(request).ToResult();
		   })
		   .WithOpenApi()
		   .WithTags("Students")
		   .RequireAuthorization()
		   .Accepts<string>("text/plain")
		   .Produces(StatusCodes.Status200OK)
		   .Produces<RequestResult>(StatusCodes.Status400BadRequest)
		   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
		   .Produces<RequestResult>(StatusCodes.Status404NotFound)
		   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
	}
}

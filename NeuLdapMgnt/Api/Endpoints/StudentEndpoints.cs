using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Api.LdapServiceExtensions;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class StudentEndpoints {
	public static void MapStudentEndpoints(this WebApplication app) {
		app.MapGet("/api/students", (LdapService ldap, HttpRequest request) =>
				ldap.GetAllEntities<Student>().RenewToken(request).ToResult()
			)
			.WithOpenApi()
			.WithTags("Students")
			.WithDescription("### Returns all entities that have the type \"*Student*\".")
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
			.WithDescription("### Returns an entity that has the type \"*Student*\" and the specified UID.")
			.RequireAuthorization()
			.Produces<Student>()
			.Produces<RequestResult>(StatusCodes.Status400BadRequest)
			.Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
			.Produces<RequestResult>(StatusCodes.Status404NotFound)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapPost("/api/students", async (LdapService ldap, HttpRequest request, string? pwd) => {
				var result = await ModelValidator.ValidateRequest<Student>(request);
				if (result.IsFailure())
					return result.RenewToken(request).ToResult();

				Student student = result.Value!;
				if (!string.IsNullOrEmpty(student.Password))
					student.SetPassword(student.Password);

				bool setPass = bool.TryParse(pwd, out var value) && value;

				return ldap.TryAddEntity(student, student.Id.ToString(), setPass).RenewToken(request).ToResult();
			})
			.WithOpenApi()
			.WithTags("Students")
			.WithDescription(
				"""
				### Adds a new entity with the type "*Student*".

				If the "**pwd**" URL parameter is set to "**true**" then the plain text password included in the object will be hashed and stored as well.
				"""
			)
			.RequireAuthorization()
			.Accepts<Student>("application/json")
			.Produces(StatusCodes.Status201Created)
			.Produces<RequestResult>(StatusCodes.Status400BadRequest)
			.Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
			.Produces<RequestResult>(StatusCodes.Status409Conflict)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapPut("/api/students/{id}", async (LdapService ldap, HttpRequest request, string id, string? pwd) => {
				var result = await ModelValidator.ValidateRequest<Student>(request);
				if (result.IsFailure())
					return result.RenewToken(request).ToResult();

				Student student = result.Value!;
				if (!string.IsNullOrEmpty(student.Password))
					student.SetPassword(student.Password);

				bool setPass = bool.TryParse(pwd, out var value) && value;
				return ldap.TryModifyEntity(student, id, setPass).RenewToken(request).ToResult();
			})
			.WithOpenApi()
			.WithTags("Students")
			.WithDescription(
				"""
				### Overwrites an entity with the type "*Student*".

				If the "**pwd**" URL parameter is set to "**true**" then the plain text password included in the object will be hashed and updated as well.
				"""
			)
			.RequireAuthorization()
			.Accepts<Student>("application/json")
			.Produces<RequestResult>()
			.Produces<RequestResult>(StatusCodes.Status400BadRequest)
			.Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
			.Produces<RequestResult>(StatusCodes.Status404NotFound)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapDelete("/api/students/{id}", (LdapService ldap, HttpRequest request, string id) =>
				ldap.TryDeleteEntity<Student>(id).RenewToken(request).ToResult()
			)
			.WithOpenApi()
			.WithTags("Students")
			.WithDescription("### Deletes an entity that has the type \"*Student*\" and the specified UID.")
			.RequireAuthorization()
			.Produces<RequestResult>()
			.Produces<RequestResult>(StatusCodes.Status400BadRequest)
			.Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
			.Produces<RequestResult>(StatusCodes.Status404NotFound)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
	}
}

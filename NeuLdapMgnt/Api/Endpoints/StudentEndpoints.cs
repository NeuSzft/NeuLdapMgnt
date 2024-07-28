using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NeuLdapMgnt.Api.LdapServiceExtensions;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class StudentEndpoints {
	public static void MapStudentEndpoints(this IEndpointRouteBuilder app) {
		app.MapGet("/api/students", (LdapService ldap, HttpRequest request) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				return ldap.GetAllEntities<Student>().RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Students")
			.WithDescription("### Returns all entities that have the type \"*Student*\".")
			.Produces<RequestResult<Student>>(StatusCodes.Status207MultiStatus)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapGet("/api/students/{id}", (LdapService ldap, HttpRequest request, string id) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				var result = ldap.TryGetEntity<Student>(id);
				return result.RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Students")
			.WithDescription("### Returns an entity that has the type \"*Student*\" and the specified UID.")
			.Produces<Student>()
			.Produces<RequestResult>(StatusCodes.Status400BadRequest)
			.Produces<RequestResult>(StatusCodes.Status404NotFound)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapPost("/api/students", async (LdapService ldap, HttpRequest request, string? pwd) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				var result = await ModelValidator.ValidateRequest<Student>(request);
				if (result.IsFailureOrEmpty())
					return result.RenewToken(request).ToResult();

				var student = result.Value;
				if (!string.IsNullOrEmpty(student.Password))
					student.SetPassword(student.Password);

				bool setPass = bool.TryParse(pwd, out bool value) && value;
				return ldap.TryAddEntity(student, student.Id.ToString(), setPass).RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Students")
			.WithDescription(
				"""
				### Adds a new entity with the type "*Student*".

				If the "**pwd**" URL parameter is set to "**true**" then the plain text password included in the object will be hashed and stored as well.
				"""
			)
			.Accepts<Student>(MediaTypeNames.Application.Json)
			.Produces(StatusCodes.Status201Created)
			.Produces<RequestResult>(StatusCodes.Status400BadRequest)
			.Produces<RequestResult>(StatusCodes.Status409Conflict)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapPut("/api/students/{id}", async (LdapService ldap, HttpRequest request, string id, string? pwd) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				var result = await ModelValidator.ValidateRequest<Student>(request);
				if (result.IsFailureOrEmpty())
					return result.RenewToken(request).ToResult();

				var student = result.Value;
				if (!string.IsNullOrEmpty(student.Password))
					student.SetPassword(student.Password);

				bool setPass = bool.TryParse(pwd, out bool value) && value;
				return ldap.TryModifyEntity(student, id, setPass).RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Students")
			.WithDescription(
				"""
				### Overwrites an entity with the type "*Student*".

				If the "**pwd**" URL parameter is set to "**true**" then the plain text password included in the object will be hashed and updated as well.
				"""
			)
			.Accepts<Student>(MediaTypeNames.Application.Json)
			.Produces<RequestResult>()
			.Produces<RequestResult>(StatusCodes.Status400BadRequest)
			.Produces<RequestResult>(StatusCodes.Status404NotFound)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapDelete("/api/students/{id}", (LdapService ldap, HttpRequest request, string id) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				return ldap.TryDeleteEntity<Student>(id).RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Students")
			.WithDescription("### Deletes an entity that has the type \"*Student*\" and the specified UID.")
			.Produces<RequestResult>()
			.Produces<RequestResult>(StatusCodes.Status400BadRequest)
			.Produces<RequestResult>(StatusCodes.Status404NotFound)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
	}
}

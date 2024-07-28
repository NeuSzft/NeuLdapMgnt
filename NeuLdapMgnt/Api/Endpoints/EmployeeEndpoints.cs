using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NeuLdapMgnt.Api.LdapServiceExtensions;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class EmployeeEndpoints {
	public static void MapEmployeeEndpoints(this IEndpointRouteBuilder app) {
		app.MapGet("/api/employees", (LdapService ldap, HttpRequest request) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				return ldap.GetAllEntities<Employee>().RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Employees")
			.WithDescription("### Returns all entities that have the type \"*Employee*\".")
			.Produces<RequestResult<Employee>>(StatusCodes.Status207MultiStatus)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapGet("/api/employees/{id}", (LdapService ldap, HttpRequest request, string id) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				var result = ldap.TryGetEntity<Employee>(id);
				return result.RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Employees")
			.WithDescription("### Returns an entity that has the type \"*Employee*\" and the specified UID.")
			.Produces<Employee>()
			.Produces<RequestResult>(StatusCodes.Status400BadRequest)
			.Produces<RequestResult>(StatusCodes.Status404NotFound)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapPost("/api/employees", async (LdapService ldap, HttpRequest request, string? pwd) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				var result = await ModelValidator.ValidateRequest<Employee>(request);
				if (result.IsFailureOrEmpty())
					return result.RenewToken(request).ToResult();

				var employee = result.Value;
				if (!string.IsNullOrEmpty(employee.Password))
					employee.SetPassword(employee.Password);

				bool setPass = bool.TryParse(pwd, out bool value) && value;
				return ldap.TryAddEntity(employee, employee.Id, setPass).RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Employees")
			.WithDescription(
				"""
				### Adds a new entity with the type "*Employee*".

				If the "**pwd**" URL parameter is set to "**true**" then the plain text password included in the object will be hashed and stored as well.
				"""
			)
			.Accepts<Employee>(MediaTypeNames.Application.Json)
			.Produces(StatusCodes.Status201Created)
			.Produces<RequestResult>(StatusCodes.Status400BadRequest)
			.Produces<RequestResult>(StatusCodes.Status409Conflict)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapPut("/api/employees/{id}", async (LdapService ldap, HttpRequest request, string id, string? pwd) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				var result = await ModelValidator.ValidateRequest<Employee>(request);
				if (result.IsFailureOrEmpty())
					return result.RenewToken(request).ToResult();

				var employee = result.Value;
				if (!string.IsNullOrEmpty(employee.Password))
					employee.SetPassword(employee.Password);

				bool setPass = bool.TryParse(pwd, out bool value) && value;
				return ldap.TryModifyEntity(employee, id, setPass).RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Employees")
			.WithDescription(
				"""
				### Overwrites an entity with the type "*Employee*".

				If the "**pwd**" URL parameter is set to "**true**" then the plain text password included in the object will be hashed and updated as well.
				"""
			)
			.Accepts<Employee>(MediaTypeNames.Application.Json)
			.Produces<RequestResult>()
			.Produces<RequestResult>(StatusCodes.Status400BadRequest)
			.Produces<RequestResult>(StatusCodes.Status404NotFound)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapDelete("/api/employees/{id}", (LdapService ldap, HttpRequest request, string id) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				return ldap.TryDeleteEntity<Employee>(id).RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Employees")
			.WithDescription("### Deletes an entity that has the type \"*Employee*\" and the specified UID.")
			.Produces<RequestResult>()
			.Produces<RequestResult>(StatusCodes.Status400BadRequest)
			.Produces<RequestResult>(StatusCodes.Status404NotFound)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
	}
}

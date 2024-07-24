using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Api.LdapServiceExtensions;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class EmployeeEndpoints {
	public static void MapEmployeeEndpoints(this WebApplication app) {
		app.MapGet("/api/employees", (LdapService ldap, HttpRequest request) =>
				ldap.GetAllEntities<Employee>().RenewToken(request).ToResult()
			)
			.WithOpenApi()
			.WithTags("Employees")
			.WithDescription("### Returns all entities that have the type \"*Employee*\".")
			.RequireAuthorization()
			.Produces<RequestResult<Employee>>(StatusCodes.Status207MultiStatus)
			.Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapGet("/api/employees/{id}", (LdapService ldap, HttpRequest request, string id) => {
				var result = ldap.TryGetEntity<Employee>(id);
				return result.RenewToken(request).ToResult();
			})
			.WithOpenApi()
			.WithTags("Employees")
			.WithDescription("### Returns an entity that has the type \"*Employee*\" and the specified UID.")
			.RequireAuthorization()
			.Produces<Employee>()
			.Produces<RequestResult>(StatusCodes.Status400BadRequest)
			.Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
			.Produces<RequestResult>(StatusCodes.Status404NotFound)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapPost("/api/employees", async (LdapService ldap, HttpRequest request, string? pwd) => {
				var result = await ModelValidator.ValidateRequest<Employee>(request);
				if (result.IsFailureOrEmpty())
					return result.RenewToken(request).ToResult();

				Employee employee = result.Value;
				if (!string.IsNullOrEmpty(employee.Password))
					employee.SetPassword(employee.Password);

				bool setPass = bool.TryParse(pwd, out bool value) && value;
				return ldap.TryAddEntity(employee, employee.Id, setPass).RenewToken(request).ToResult();
			})
			.WithOpenApi()
			.WithTags("Employees")
			.WithDescription(
				"""
				### Adds a new entity with the type "*Employee*".

				If the "**pwd**" URL parameter is set to "**true**" then the plain text password included in the object will be hashed and stored as well.
				"""
			)
			.RequireAuthorization()
			.Accepts<Employee>("application/json")
			.Produces(StatusCodes.Status201Created)
			.Produces<RequestResult>(StatusCodes.Status400BadRequest)
			.Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
			.Produces<RequestResult>(StatusCodes.Status409Conflict)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapPut("/api/employees/{id}", async (LdapService ldap, HttpRequest request, string id, string? pwd) => {
				var result = await ModelValidator.ValidateRequest<Employee>(request);
				if (result.IsFailureOrEmpty())
					return result.RenewToken(request).ToResult();

				Employee employee = result.Value;
				if (!string.IsNullOrEmpty(employee.Password))
					employee.SetPassword(employee.Password);

				bool setPass = bool.TryParse(pwd, out bool value) && value;
				return ldap.TryModifyEntity(employee, id, setPass).RenewToken(request).ToResult();
			})
			.WithOpenApi()
			.WithTags("Employees")
			.WithDescription(
				"""
				### Overwrites an entity with the type "*Employee*".

				If the "**pwd**" URL parameter is set to "**true**" then the plain text password included in the object will be hashed and updated as well.
				"""
			)
			.RequireAuthorization()
			.Accepts<Employee>("application/json")
			.Produces<RequestResult>()
			.Produces<RequestResult>(StatusCodes.Status400BadRequest)
			.Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
			.Produces<RequestResult>(StatusCodes.Status404NotFound)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapDelete("/api/employees/{id}", (LdapService ldap, HttpRequest request, string id) =>
				ldap.TryDeleteEntity<Employee>(id).RenewToken(request).ToResult()
			)
			.WithOpenApi()
			.WithTags("Employees")
			.WithDescription("### Deletes an entity that has the type \"*Employee*\" and the specified UID.")
			.RequireAuthorization()
			.Produces<RequestResult>()
			.Produces<RequestResult>(StatusCodes.Status400BadRequest)
			.Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
			.Produces<RequestResult>(StatusCodes.Status404NotFound)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
	}
}

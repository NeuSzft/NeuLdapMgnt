using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NeuLdapMgnt.Api.LdapServiceExtensions;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class DbManagementEndpoints {
	public static void MapDbManagementEndpoints(this IEndpointRouteBuilder app) {
		app.MapGet("/api/database", (LdapService ldap, HttpRequest request) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				return ldap.ExportDatabase().RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Database Management")
			.WithDescription("### Returns a JSON dump of the database.")
			.Produces<RequestResult<LdapDbDump>>(StatusCodes.Status207MultiStatus)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapPost("/api/database", async (LdapService ldap, HttpRequest request) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				var result = await ModelValidator.ValidateRequest<LdapDbDump>(request);
				return result.IsSuccessAndNotEmpty()
					? ldap.ImportDatabase(result.Value, false).RenewToken(request).ToResult()
					: result.RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Database Management")
			.WithDescription("### Imports a JSON dump into the database without overwriting existing entities or groups, only the values of the already existing key-value pairs are overwritten.")
			.Accepts<LdapDbDump>(MediaTypeNames.Application.Json)
			.Produces<RequestResult>(StatusCodes.Status207MultiStatus)
			.Produces<RequestResult>(StatusCodes.Status400BadRequest)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapPut("/api/database", async (LdapService ldap, HttpRequest request) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				var result = await ModelValidator.ValidateRequest<LdapDbDump>(request);
				return result.IsSuccessAndNotEmpty()
					? ldap.ImportDatabase(result.Value, true).RenewToken(request).ToResult()
					: result.RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Database Management")
			.WithDescription("### Imports a JSON dump into the database and overwrites already existing entities, groups and key-value pairs.")
			.Accepts<LdapDbDump>(MediaTypeNames.Application.Json)
			.Produces<RequestResult>(StatusCodes.Status207MultiStatus)
			.Produces<RequestResult>(StatusCodes.Status400BadRequest)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapDelete("/api/database", (LdapService ldap, HttpRequest request) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				return ldap.EraseDatabase().RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Database Management")
			.WithDescription("### Recursively deletes all elements of the LDAP domain.")
			.Produces<RequestResult>()
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
	}
}

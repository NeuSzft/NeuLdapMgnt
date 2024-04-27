using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Api.LdapServiceExtensions;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class DbManagementEndpoints {
	public static void MapDbManagementEndpoints(this WebApplication app) {
		app.MapGet("/api/database", (LdapService ldap, HttpRequest request) =>
			   ldap.ExportDatabase().RenewToken(request).ToResult()
		   )
		   .WithOpenApi()
		   .WithTags("Database Management")
		   .WithDescription("### Returns a JSON dump of the database.")
		   .RequireAuthorization()
		   .Produces<RequestResult>(StatusCodes.Status207MultiStatus)
		   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
		   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapPost("/api/database", async (LdapService ldap, HttpRequest request) => {
			   var result = await ModelValidator.ValidateRequest<LdapDbDump>(request);
			   if (result.IsFailure())
				   return result.RenewToken(request).ToResult();

			   return ldap.ImportDatabase(result.GetValue()!, false).RenewToken(request).ToResult();
		   })
		   .WithOpenApi()
		   .WithTags("Database Management")
		   .WithDescription("### Imports a JSON dump into the database without overwriting existing entities or groups, only the values of the already existing key-value pairs are overwritten.")
		   .RequireAuthorization()
		   .Accepts<LdapDbDump>("application/json")
		   .Produces<RequestResult>(StatusCodes.Status207MultiStatus)
		   .Produces<RequestResult>(StatusCodes.Status400BadRequest)
		   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
		   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapPut("/api/database", async (LdapService ldap, HttpRequest request) => {
			   var result = await ModelValidator.ValidateRequest<LdapDbDump>(request);
			   if (result.IsFailure())
				   return result.RenewToken(request).ToResult();

			   return ldap.ImportDatabase(result.GetValue()!, true).RenewToken(request).ToResult();
		   })
		   .WithOpenApi()
		   .WithTags("Database Management")
		   .WithDescription("### Imports a JSON dump into the database and overwrites already existing entities, groups and key-value pairs.")
		   .RequireAuthorization()
		   .Accepts<LdapDbDump>("application/json")
		   .Produces<RequestResult>(StatusCodes.Status207MultiStatus)
		   .Produces<RequestResult>(StatusCodes.Status400BadRequest)
		   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
		   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapDelete("/api/database", (LdapService ldap, HttpRequest request) =>
			   ldap.EraseDatabase().RenewToken(request).ToResult()
		   )
		   .WithOpenApi()
		   .WithTags("Database Management")
		   .WithDescription("### Recursively deletes all elements of the LDAP domain.")
		   .RequireAuthorization()
		   .Produces<RequestResult>()
		   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
		   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
	}
}

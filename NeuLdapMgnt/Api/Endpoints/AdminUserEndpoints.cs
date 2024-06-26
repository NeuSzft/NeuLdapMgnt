using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Api.LdapServiceExtensions;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class AdminUserEndpoints {
	public static void MapAdminUserEndpoints(this WebApplication app) {
		app.MapGet("/api/admins", (LdapService ldap, HttpRequest request) => {
			   IEnumerable<string> uids = ldap.GetMembersOfGroup("admin");
			   return new RequestResult<IEnumerable<string>>().SetValues(uids).RenewToken(request).ToResult();
		   })
		   .WithOpenApi()
		   .WithTags("Admin Users")
		   .WithDescription("### Returns all UIDs that are part of the \"*admin*\" group.")
		   .RequireAuthorization()
		   .Produces<RequestResult<string[]>>()
		   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
		   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapPost("/api/admins/{id}", (LdapService ldap, HttpRequest request, string id) => {
			   var result = ldap.TryAddEntityToGroup("admin", id);
			   return result.RenewToken(request).ToResult();
		   })
		   .WithOpenApi()
		   .WithTags("Admin Users")
		   .WithDescription("### Adds a UID to the \"*admin*\" group.")
		   .RequireAuthorization()
		   .Produces<RequestResult>()
		   .Produces<RequestResult>(StatusCodes.Status400BadRequest)
		   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
		   .Produces<RequestResult>(StatusCodes.Status409Conflict)
		   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapDelete("/api/admins/{id}", (LdapService ldap, HttpRequest request, string id) => {
			   var result = ldap.TryRemoveEntityFromGroup("admin", id);
			   return result.RenewToken(request).ToResult();
		   })
		   .WithOpenApi()
		   .WithTags("Admin Users")
		   .WithDescription("### Removes a UID from the \"*admin*\" group.")
		   .RequireAuthorization()
		   .Produces<RequestResult>()
		   .Produces<RequestResult>(StatusCodes.Status400BadRequest)
		   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
		   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapGet("/api/defaultadmin/enabled", (LdapService ldap, HttpRequest request) => {
			   string? value = ldap.GetValue(Authenticator.DefaultAdminEnabledValueName, out var error);
			   if (error is not null)
				   return new RequestResult<bool>().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error).RenewToken(request).ToResult();
			   return new RequestResult<bool>().SetValues(bool.TryParse(value, out var enable) && enable).RenewToken(request).ToResult();
		   })
		   .WithOpenApi()
		   .WithTags("Admin Users")
		   .WithDescription("### Returns whether the default admin is enabled or not.")
		   .RequireAuthorization()
		   .Produces<RequestResult<bool>>()
		   .Produces<RequestResult<bool>>(StatusCodes.Status400BadRequest)
		   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
		   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapPut("/api/defaultadmin/enabled", async (LdapService ldap, HttpRequest request) => {
			   using StreamReader reader = new(request.Body);
			   string             value  = await reader.ReadToEndAsync();

			   if (!bool.TryParse(value, out var enable))
				   return new RequestResult().SetStatus(StatusCodes.Status400BadRequest).SetErrors($"'{value}' is not a valid boolean.").RenewToken(request).ToResult();

			   ldap.SetValue(Authenticator.DefaultAdminEnabledValueName, enable.ToString(), out var error);
			   if (error is not null)
				   return new RequestResult().SetStatus(StatusCodes.Status503ServiceUnavailable).SetErrors(error).RenewToken(request).ToResult();

			   return new RequestResult().RenewToken(request).ToResult();
		   })
		   .WithOpenApi()
		   .WithTags("Admin Users")
		   .WithDescription("### Sets whether the default admin is enabled or not.")
		   .RequireAuthorization()
		   .Accepts<bool>("text/plain")
		   .Produces<RequestResult>()
		   .Produces<RequestResult>(StatusCodes.Status400BadRequest)
		   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
		   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapPut("/api/defaultadmin/password", async (LdapService ldap, HttpRequest request) => {
			   using StreamReader reader   = new(request.Body);
			   string             password = await reader.ReadToEndAsync();

			   ldap.SetValue(Authenticator.DefaultAdminPasswordValueName, Utils.BCryptHashPassword(password), out var error);
			   if (error is not null)
				   return new RequestResult().SetStatus(StatusCodes.Status503ServiceUnavailable).SetErrors(error).RenewToken(request).ToResult();

			   return new RequestResult().RenewToken(request).ToResult();
		   })
		   .WithOpenApi()
		   .WithTags("Admin Users")
		   .WithDescription("### Sets the password of the default admin.")
		   .RequireAuthorization()
		   .Accepts<string>("text/plain")
		   .Produces<RequestResult>()
		   .Produces<RequestResult>(StatusCodes.Status400BadRequest)
		   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
		   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
	}
}

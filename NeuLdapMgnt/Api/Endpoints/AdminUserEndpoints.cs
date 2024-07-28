using System.IO;
using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NeuLdapMgnt.Api.LdapServiceExtensions;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class AdminUserEndpoints {
	public static void MapAdminUserEndpoints(this IEndpointRouteBuilder app) {
		app.MapGet("/api/admins", (LdapService ldap, HttpRequest request) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				string[] ids = ldap.GetAllEntities<Employee>().Values.Where(x => x.IsAdmin).Select(x => x.Id).ToArray();
				return new RequestResult<string>().SetValues(ids).RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Admin Users")
			.WithDescription("### Returns all UIDs that are part of the \"*admin*\" group.")
			.Produces<RequestResult<string>>()
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapGet("/api/default-admin/enabled", (LdapService ldap, HttpRequest request) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				string? value = ldap.GetValue(Authenticator.DefaultAdminEnabledValueName, out string? error);
				return error is null
					? new RequestResult<bool>().SetValues(bool.TryParse(value, out bool enable) && enable).RenewToken(request).ToResult()
					: new RequestResult<bool>().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error).RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Admin Users")
			.WithDescription("### Returns whether the default admin is enabled or not.")
			.Produces<RequestResult<bool>>()
			.Produces<RequestResult<bool>>(StatusCodes.Status400BadRequest)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapPut("/api/default-admin/enabled", async (LdapService ldap, HttpRequest request) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				using StreamReader reader = new(request.Body);
				string             value  = await reader.ReadToEndAsync();

				if (!bool.TryParse(value, out bool enable))
					return new RequestResult().SetStatus(StatusCodes.Status400BadRequest).SetErrors($"'{value}' is not a valid boolean.").RenewToken(request).ToResult();

				ldap.SetValue(Authenticator.DefaultAdminEnabledValueName, enable.ToString(), out string? error);
				return error is null
					? new RequestResult().RenewToken(request).ToResult()
					: new RequestResult().SetStatus(StatusCodes.Status503ServiceUnavailable).SetErrors(error).RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Admin Users")
			.WithDescription("### Sets whether the default admin is enabled or not.")
			.Accepts<bool>(MediaTypeNames.Text.Plain)
			.Produces<RequestResult>()
			.Produces<RequestResult>(StatusCodes.Status400BadRequest)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapPut("/api/default-admin/password", async (LdapService ldap, HttpRequest request) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				using StreamReader reader   = new(request.Body);
				string             password = await reader.ReadToEndAsync();

				ldap.SetValue(Authenticator.DefaultAdminPasswordValueName, Utils.BCryptHashPassword(password), out string? error);
				return error is null
					? new RequestResult().RenewToken(request).ToResult()
					: new RequestResult().SetStatus(StatusCodes.Status503ServiceUnavailable).SetErrors(error).RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Admin Users")
			.WithDescription("### Sets the password of the default admin.")
			.Accepts<string>(MediaTypeNames.Text.Plain)
			.Produces<RequestResult>()
			.Produces<RequestResult>(StatusCodes.Status400BadRequest)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
	}
}

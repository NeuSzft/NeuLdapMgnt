using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints {
	public static class TeacherEndpoints {
		public static void MapTeacherEndpoints(this WebApplication app) {
			app.MapGet("/api/teachers", (LdapService ldap, HttpRequest request) =>
				   ldap.GetAllEntities<Teacher>().RenewToken(request).ToResult()
			   ).WithOpenApi()
			   .WithTags("Teachers")
			   .RequireAuthorization()
			   .Produces<RequestResult<Teacher>>(StatusCodes.Status207MultiStatus)
			   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
			   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

			app.MapGet("/api/teachers/{id}", (LdapService ldap, HttpRequest request, string id) => {
				   var result = ldap.TryGetEntity<Teacher>(id);
				   return result.RenewToken(request).ToResult();
			   }).WithOpenApi()
			   .WithTags("Teachers")
			   .RequireAuthorization()
			   .Produces<Teacher>()
			   .Produces<RequestResult>(StatusCodes.Status400BadRequest)
			   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
			   .Produces<RequestResult>(StatusCodes.Status404NotFound)
			   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

			app.MapPost("/api/teachers", async (LdapService ldap, HttpRequest request) => {
				   var result = await ModelValidator.ValidateRequest<Teacher>(request);
				   if (result.IsFailure())
					   return result.RenewToken(request).ToResult();
				   return ldap.TryAddEntity(result.GetValue()!, result.GetValue()!.Id).RenewToken(request).ToResult();
			   }).WithOpenApi()
			   .WithTags("Teachers")
			   .RequireAuthorization()
			   .Accepts<Teacher>("application/json")
			   .Produces(StatusCodes.Status201Created)
			   .Produces<RequestResult>(StatusCodes.Status400BadRequest)
			   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
			   .Produces<RequestResult>(StatusCodes.Status409Conflict)
			   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

			app.MapPut("/api/teachers/{id}", async (LdapService ldap, HttpRequest request, string id) => {
				   var result = await ModelValidator.ValidateRequest<Teacher>(request);
				   if (result.IsFailure())
					   return result.RenewToken(request).ToResult();
				   return ldap.TryModifyEntity(result.GetValue()!, id).RenewToken(request).ToResult();
			   }).WithOpenApi()
			   .WithTags("Teachers")
			   .RequireAuthorization()
			   .Accepts<Teacher>("application/json")
			   .Produces(StatusCodes.Status200OK)
			   .Produces<RequestResult>(StatusCodes.Status400BadRequest)
			   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
			   .Produces<RequestResult>(StatusCodes.Status404NotFound)
			   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

			app.MapDelete("/api/teachers/{id}", (LdapService ldap, HttpRequest request, string id) =>
				   ldap.TryDeleteEntity<Teacher>(id).RenewToken(request).ToResult()
			   ).WithOpenApi()
			   .WithTags("Teachers")
			   .RequireAuthorization()
			   .Produces(StatusCodes.Status200OK)
			   .Produces<RequestResult>(StatusCodes.Status400BadRequest)
			   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
			   .Produces<RequestResult>(StatusCodes.Status404NotFound)
			   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
		}
	}
}

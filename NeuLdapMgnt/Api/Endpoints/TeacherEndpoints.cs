using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Api.LdapServiceExtensions;
using NeuLdapMgnt.Models;
using NeuLdapMgnt.Models.CustomValidationAttributes;

namespace NeuLdapMgnt.Api.Endpoints {
	public static class TeacherEndpoints {
		public static void MapTeacherEndpoints(this WebApplication app) {
			app.MapGet("/api/teachers", (LdapService ldap, HttpRequest request) =>
				   ldap.GetAllEntities<Teacher>().RenewToken(request).ToResult()
			   ).WithOpenApi()
			   .WithTags("Teachers")
			   .WithDescription("### Returns all entities that have the type \"*Teacher*\".")
			   .RequireAuthorization()
			   .Produces<RequestResult<Teacher>>(StatusCodes.Status207MultiStatus)
			   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
			   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

			app.MapGet("/api/teachers/{id}", (LdapService ldap, HttpRequest request, string id) => {
				   var result = ldap.TryGetEntity<Teacher>(id);
				   return result.RenewToken(request).ToResult();
			   }).WithOpenApi()
			   .WithTags("Teachers")
			   .WithDescription("### Returns an entity that has the type \"*Teacher*\" and the specified UID.")
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
			   .WithDescription("### Adds a new entity with the type \"*Teacher*\".")
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
			   .WithDescription("### Overwrites an entity that has the type \"*Teacher*\" and the specified UID.")
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
			   .WithDescription("### Deletes an entity that has the type \"*Teacher*\" and the specified UID.")
			   .RequireAuthorization()
			   .Produces(StatusCodes.Status200OK)
			   .Produces<RequestResult>(StatusCodes.Status400BadRequest)
			   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
			   .Produces<RequestResult>(StatusCodes.Status404NotFound)
			   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

			app.MapPut("/api/teachers/{id}/password", async (LdapService ldap, HttpRequest request, string id) => {
				   using StreamReader reader   = new(request.Body);
				   string             password = await reader.ReadToEndAsync();

				   var validation = ModelValidator.ValidateValue(password, new PasswordAttribute());
				   if (validation.IsFailure())
					   return validation.RenewToken(request).ToResult();

				   var response = ldap.TryGetEntity<Teacher>(id, true);
				   if (response.IsFailure())
					   return response.RenewToken(request).ToResult();

				   Teacher teacher = response.GetValue()!;
				   teacher.Password = new UserPassword(password, 16).ToString();

				   return ldap.TryModifyEntity(teacher, id, true).RenewToken(request).ToResult();
			   })
			   .WithOpenApi()
			   .WithTags("Teachers")
			   .WithDescription("### Updates the password of an entity that has the type \"*Teacher*\" and the specified UID.")
			   .RequireAuthorization()
			   .Accepts<string>("text/plain")
			   .Produces(StatusCodes.Status200OK)
			   .Produces<RequestResult>(StatusCodes.Status400BadRequest)
			   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
			   .Produces<RequestResult>(StatusCodes.Status404NotFound)
			   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
		}
	}
}

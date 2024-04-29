using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Api.LdapServiceExtensions;
using NeuLdapMgnt.Models;

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

			app.MapPost("/api/teachers", async (LdapService ldap, HttpRequest request, string? pwd) => {
				   var result = await ModelValidator.ValidateRequest<Teacher>(request);
				   if (result.IsFailure())
					   return result.RenewToken(request).ToResult();

				   Teacher teacher = result.GetValue()!;
				   if (!string.IsNullOrEmpty(teacher.Password))
					   teacher.SetPassword(teacher.Password);

				   bool setPass = bool.TryParse(pwd, out var value) && value;
				   return ldap.TryAddEntity(teacher, teacher.Id, setPass).RenewToken(request).ToResult();
			   }).WithOpenApi()
			   .WithTags("Teachers")
			   .WithDescription(
				   """
				   ### Adds a new entity with the type "*Teacher*".

				   If the "**pwd**" URL parameter is set to "**true**" then the plain text password included in the object will be hashed and stored as well.
				   """
			   )
			   .RequireAuthorization()
			   .Accepts<Teacher>("application/json")
			   .Produces(StatusCodes.Status201Created)
			   .Produces<RequestResult>(StatusCodes.Status400BadRequest)
			   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
			   .Produces<RequestResult>(StatusCodes.Status409Conflict)
			   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

			app.MapPut("/api/teachers/{id}", async (LdapService ldap, HttpRequest request, string id, string? pwd) => {
				   var result = await ModelValidator.ValidateRequest<Teacher>(request);
				   if (result.IsFailure())
					   return result.RenewToken(request).ToResult();

				   Teacher teacher = result.GetValue()!;
				   if (!string.IsNullOrEmpty(teacher.Password))
					   teacher.SetPassword(teacher.Password);

				   bool setPass = bool.TryParse(pwd, out var value) && value;
				   return ldap.TryModifyEntity(teacher, id, setPass).RenewToken(request).ToResult();
			   }).WithOpenApi()
			   .WithTags("Teachers")
			   .WithDescription(
				   """
				   ### Overwrites an entity with the type "*Teacher*".

				   If the "**pwd**" URL parameter is set to "**true**" then the plain text password included in the object will be hashed and updated as well.
				   """
			   )
			   .RequireAuthorization()
			   .Accepts<Teacher>("application/json")
			   .Produces<RequestResult>()
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
			   .Produces<RequestResult>()
			   .Produces<RequestResult>(StatusCodes.Status400BadRequest)
			   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
			   .Produces<RequestResult>(StatusCodes.Status404NotFound)
			   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
		}
	}
}

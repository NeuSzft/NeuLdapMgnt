using System;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NeuLdapMgnt.Api.LdapServiceExtensions;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class ClassEndpoints {
	public static void MapClassEndpoints(this IEndpointRouteBuilder app) {
		app.MapGet("/api/classes", (LdapService ldap, HttpRequest request) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				string?  value   = ldap.GetValue("classes", out _);
				string[] classes = value?.Split(';') ?? [ ];
				return new RequestResult<string>().SetValues(classes).RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Classes")
			.WithDescription("### Returns the classes that students can belong to and a teacher can be in charge of.")
			.Produces<RequestResult<string>>()
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);

		app.MapPut("/api/classes", async (LdapService ldap, HttpRequest request) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				string[] classes;
				try {
					classes = await JsonSerializer.DeserializeAsync<string[]>(request.Body) ?? [ ];
				}
				catch (Exception e) {
					return new RequestResult().SetStatus(StatusCodes.Status400BadRequest).SetErrors(e.GetError()).RenewToken(request).ToResult();
				}

				bool success = classes.Length == 0
					? ldap.UnsetValue("classes", out string? error, true)
					: ldap.SetValue("classes", string.Join(';', classes), out error);

				RequestResult result = new();
				if (error is not null)
					result.SetErrors(error);

				return result.SetStatus(success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest).RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Classes")
			.WithDescription("### Sets the classes that students can belong to and a teacher can be in charge of.")
			.Accepts<string[]>(MediaTypeNames.Application.Json)
			.Produces<RequestResult>()
			.Produces<RequestResult>(StatusCodes.Status400BadRequest)
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
	}
}

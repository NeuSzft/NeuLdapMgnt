using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.IdentityModel.Tokens;

namespace NeuLdapMgnt.Api.Endpoints;

public static class TestingEndpoints {
	public static void MapTestingEndpoints(this IEndpointRouteBuilder app) {
		app.MapGet("/api/testing/get-token", () =>
				Results.Text(Authenticator.CreateToken("testuser"))
			)
			.WithOpenApiJwtAuth()
			.WithTags("Testing")
			.Produces<string>();

		app.MapGet("/api/testing/get-exp-token", () => {
				JwtSecurityToken jwt = new(
					Program.ServiceName,
					"testuser",
					null,
					DateTime.Now.Subtract(TimeSpan.FromHours(2)),
					DateTime.Now.Subtract(TimeSpan.FromHours(1)),
					new SigningCredentials(Program.SecurityKey, SecurityAlgorithms.HmacSha512)
				);
				return Results.Text(new JwtSecurityTokenHandler().WriteToken(jwt));
			})
			.WithOpenApiJwtAuth()
			.WithTags("Testing")
			.Produces<string>();

		app.MapGet("/api/testing/check-token", (HttpRequest request) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				return Results.Ok();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Testing")
			.Produces<string>();

		app.MapPost("/api/testing/throw-exception", async (HttpRequest request) => {
				using StreamReader reader = new(request.Body);
				string             body   = await reader.ReadToEndAsync();
				throw new Exception(string.IsNullOrWhiteSpace(body) ? "Test exception" : body);
			})
			.WithOpenApiJwtAuth()
			.WithTags("Testing")
			.Accepts<string>(MediaTypeNames.Text.Plain)
			.Produces<string>(StatusCodes.Status500InternalServerError);
	}
}

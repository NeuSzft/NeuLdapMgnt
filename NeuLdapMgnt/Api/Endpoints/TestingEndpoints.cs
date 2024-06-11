using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace NeuLdapMgnt.Api.Endpoints;

public static class TestingEndpoints {
	public static void MapTestingEndpoints(this WebApplication app) {
		app.MapGet("/api/testing/get-token", () =>
				Results.Text(Authenticator.CreateToken("testuser"))
			)
			.WithOpenApi()
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
			.WithOpenApi()
			.WithTags("Testing")
			.Produces<string>();

		app.MapGet("/api/testing/check-token", () =>
				Results.Ok()
			)
			.WithOpenApi()
			.WithTags("Testing")
			.RequireAuthorization()
			.Produces<string>();
	}
}

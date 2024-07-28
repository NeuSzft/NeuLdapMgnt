using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace NeuLdapMgnt.Api.Endpoints;

public static class AuthEndpoints {
	public static void MapAuthEndpoints(this IEndpointRouteBuilder app) {
		app.MapGet("/api/auth", Authenticator.BasicAuth)
			.WithOpenApiBasicAuth()
			.WithTags("Authentication")
			.WithDescription("### Performs Basic authentication and returns a Json Web Token on successful authentication.")
			.Produces<string>()
			.Produces<string>(StatusCodes.Status500InternalServerError)
			.Produces<string>(StatusCodes.Status503ServiceUnavailable);
	}
}

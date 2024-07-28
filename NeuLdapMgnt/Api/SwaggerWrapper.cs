using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace NeuLdapMgnt.Api;

internal static class SwaggerWrapper {
	private static readonly OpenApiSecurityRequirement BasicAuthRequirement = new() {
		{
			new OpenApiSecurityScheme {
				Reference = new OpenApiReference {
					Type = ReferenceType.SecurityScheme,
					Id   = Authenticator.Schemes.Basic
				}
			},
			[ ]
		}
	};

	private static readonly OpenApiSecurityRequirement JwtAuthRequirement = new() {
		{
			new OpenApiSecurityScheme {
				Reference = new OpenApiReference {
					Type = ReferenceType.SecurityScheme,
					Id   = Authenticator.Schemes.Jwt
				}
			},
			[ ]
		}
	};

	/// <summary>Sets the Swagger document options and security definitions for the services.</summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to set the options for.</param>
	/// <param name="docName">A URI-friendly name that uniquely identifies the OpenAPI document.</param>
	/// <param name="title">The title of the OpenAPI document.</param>
	/// <param name="version">The version of the OpenAPI document.</param>
	/// <returns>The <see cref="IServiceCollection"/> that was passed to this method.</returns>
	public static IServiceCollection AddSwaggerWrapperGen(this IServiceCollection services, string docName, string title, string version) {
		services.AddSwaggerGen(options => {
			options.SupportNonNullableReferenceTypes();

			options.SwaggerDoc(docName, new OpenApiInfo {
				Title   = title,
				Version = version
			});

			options.AddSecurityDefinition(Authenticator.Schemes.Basic, new OpenApiSecurityScheme {
				Type   = SecuritySchemeType.Http,
				In     = ParameterLocation.Header,
				Name   = "Password Authentication",
				Scheme = Authenticator.Schemes.Basic
			});
			options.AddSecurityDefinition(Authenticator.Schemes.Jwt, new OpenApiSecurityScheme {
				Type         = SecuritySchemeType.Http,
				In           = ParameterLocation.Header,
				Name         = "JWT Authentication",
				Scheme       = Authenticator.Schemes.Jwt,
				BearerFormat = "JWT"
			});
		});

		return services;
	}

	/// <summary>Registers the Swagger and SwaggerUI middlewares.</summary>
	/// <param name="app">The <see cref="IApplicationBuilder"/> to register the middlewares to.</param>
	/// <param name="docName">A URI-friendly name that uniquely identifies the document.</param>
	/// <returns>The <see cref="IApplicationBuilder"/> that was passed to this method.</returns>
	public static IApplicationBuilder UseSwaggerWrapper(this IApplicationBuilder app, string docName) {
		app.UseSwagger(options => options.RouteTemplate = "/api/docs/{documentName}/endpoints.json");
		app.UseSwaggerUI(options => {
			options.SwaggerEndpoint($"/api/docs/{docName}/endpoints.json", "API Endpoints");
			options.RoutePrefix = "api/docs";
		});

		return app;
	}

	/// <summary>Adds an OpenAPI annotation the endpoint with the <see cref="Authenticator.Schemes.Basic"/> authentication scheme.</summary>
	/// <param name="builder">The <see cref="RouteHandlerBuilder" />.</param>
	/// <returns>The <paramref name="builder"/> that was passed to the method.</returns>
	public static RouteHandlerBuilder WithOpenApiBasicAuth(this RouteHandlerBuilder builder) {
		return builder.WithOpenApi(op => {
				op.Security.Add(BasicAuthRequirement);
				return op;
			})
			.Produces<string>(StatusCodes.Status400BadRequest, MediaTypeNames.Text.Plain)
			.Produces<string>(StatusCodes.Status401Unauthorized, MediaTypeNames.Text.Plain)
			.Produces<string>(StatusCodes.Status403Forbidden, MediaTypeNames.Text.Plain);
	}

	/// <summary>Adds an OpenAPI annotation the endpoint with the <see cref="Authenticator.Schemes.Jwt"/> authentication scheme.</summary>
	/// <param name="builder">The <see cref="RouteHandlerBuilder" />.</param>
	/// <returns>The <paramref name="builder"/> that was passed to the method.</returns>
	public static RouteHandlerBuilder WithOpenApiJwtAuth(this RouteHandlerBuilder builder) {
		return builder.WithOpenApi(op => {
				op.Security.Add(JwtAuthRequirement);
				return op;
			})
			.Produces<string>(StatusCodes.Status400BadRequest, MediaTypeNames.Text.Plain)
			.Produces<string>(StatusCodes.Status401Unauthorized, MediaTypeNames.Text.Plain);
	}
}

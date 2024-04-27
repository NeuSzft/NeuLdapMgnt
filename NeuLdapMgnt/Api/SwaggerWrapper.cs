using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NeuLdapMgnt.Api;

internal static class SwaggerWrapper {
	private static readonly OpenApiSecurityRequirement BasicAuthRequirement = new() {
		{
			new OpenApiSecurityScheme {
				Reference = new OpenApiReference {
					Type = ReferenceType.SecurityScheme,
					Id = "Basic"
				}
			},
			[]
		}
	};

	private static readonly OpenApiSecurityRequirement JwtAuthRequirement = new() {
		{
			new OpenApiSecurityScheme {
				Reference = new OpenApiReference {
					Type = ReferenceType.SecurityScheme,
					Id = JwtBearerDefaults.AuthenticationScheme
				}
			},
			[]
		}
	};

	private sealed class SecurityRequirementsOperationFilter : IOperationFilter {
		public void Apply(OpenApiOperation operation, OperationFilterContext context) {
			// Do not add security scheme if it is a testing endpoint
			if (operation.Tags.All(x => x.Name != "Testing")) {
				OpenApiSecurityRequirement securityRequirement = operation.OperationId == "BasicAuth" ? BasicAuthRequirement : JwtAuthRequirement;
				if (!operation.Security.Contains(securityRequirement))
					operation.Security.Add(securityRequirement);
			}
		}
	}

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
				Title = title,
				Version = version
			});

			options.AddSecurityDefinition("Basic", new OpenApiSecurityScheme {
				Type = SecuritySchemeType.Http,
				In = ParameterLocation.Header,
				Name = "Password Authentication",
				Scheme = "Basic"
			});
			options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme {
				Type = SecuritySchemeType.Http,
				In = ParameterLocation.Header,
				Name = "JWT Authentication",
				Scheme = JwtBearerDefaults.AuthenticationScheme,
				BearerFormat = "JWT"
			});

			options.OperationFilter<SecurityRequirementsOperationFilter>();
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
}

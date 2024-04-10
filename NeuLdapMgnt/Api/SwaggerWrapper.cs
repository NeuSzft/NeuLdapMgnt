using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NeuLdapMgnt.Api;

internal static class SwaggerWrapper {
    private sealed class SecurityRequirementsOperationFilter : IOperationFilter {
        public void Apply(OpenApiOperation operation, OperationFilterContext context) {
            // Do not add security scheme if it is a testing endpoint
            if (operation.Tags.All(x => x.Name != "Testing"))
                operation.Security.Add(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme {
                            // If the name of the endpoint is "BasicAuth" then use Basic auth instead of JWT
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id   = operation.OperationId == "BasicAuth" ? "Basic" : JwtBearerDefaults.AuthenticationScheme
                            }
                        },
                        []
                    }
                });
        }
    }

    public static IServiceCollection AddSwaggerWrapperGen(this IServiceCollection services) {
        services.AddSwaggerGen(options => {
            options.SupportNonNullableReferenceTypes();

            options.SwaggerDoc("api", new OpenApiInfo {
                Title   = "Neu LDAP Management API",
                Version = "1.0.0"
            });

            options.AddSecurityDefinition("Basic", new OpenApiSecurityScheme {
                Type   = SecuritySchemeType.Http,
                In     = ParameterLocation.Header,
                Name   = "Password Authentication",
                Scheme = "Basic"
            });
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme {
                Type         = SecuritySchemeType.Http,
                In           = ParameterLocation.Header,
                Name         = "JWT Authentication",
                Scheme       = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT"
            });

            options.OperationFilter<SecurityRequirementsOperationFilter>();
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerWrapper(this IApplicationBuilder app) {
        app.UseSwagger(options => options.RouteTemplate = "/api/docs/{documentName}/endpoints.json");
        app.UseSwaggerUI(options => {
            options.SwaggerEndpoint("/api/docs/api/endpoints.json", "API Endpoints");
            options.RoutePrefix = "api/docs";
        });

        return app;
    }
}

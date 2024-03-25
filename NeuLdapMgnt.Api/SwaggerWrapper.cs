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
            operation.Security.Add(new OpenApiSecurityRequirement {
                {
                    new OpenApiSecurityScheme {
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
        app.UseSwagger();
        app.UseSwaggerUI();

        return app;
    }
}

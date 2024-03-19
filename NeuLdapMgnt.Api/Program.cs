using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NeuLdapMgnt.Api.Endpoints;

namespace NeuLdapMgnt.Api;

internal static class Program {
    public static readonly SymmetricSecurityKey SecurityKey = Utils.LoadOrCreateSecurityKey("SECURITY_KEY");

    public static readonly string TokenIssuer = Assembly.GetExecutingAssembly().FullName!;

    public static void Main(string[] args) {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Create jwt authentication scheme
        builder.Services.AddAuthentication("JwtBearer").AddJwtBearer("JwtBearer", options => {
            options.TokenValidationParameters = new TokenValidationParameters {
                ValidateAudience = false,
                ValidateIssuer   = true,
                ValidateLifetime = true,

                IssuerSigningKey = SecurityKey,
                ValidIssuers     = new[] { TokenIssuer },
                ValidAlgorithms  = new[] { SecurityAlgorithms.HmacSha256 }
            };

            options.Events = new JwtBearerEvents {
                // Try to set the jwt from cookie if Authorization header is not specified. Fail if neither are present.
                OnMessageReceived = (MessageReceivedContext context) => {
                    if (context.Request.Headers.ContainsKey("Authorization"))
                        return Task.CompletedTask;

                    if (context.Request.Cookies.TryGetValue("jwt", out var cookie))
                        context.Token = cookie;
                    else
                        context.Fail("No json web token was specified");
                    
                    return Task.CompletedTask;
                },

                // Return 401 from middleware if authentication fails
                OnChallenge = async (JwtBearerChallengeContext context) => {
                    context.HandleResponse();
                    if (context.AuthenticateFailure is not null) {
                        context.Response.StatusCode  = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync(context.ErrorDescription.DefaultIfNullOrEmpty("Invalid json web token"));
                        await context.Response.CompleteAsync();
                    }
                },
            };
        });

        // Add authorization policy that uses the previously created authentication scheme
        builder.Services.AddAuthorization(options => {
            options.AddPolicy("JwtBearerPolicy", policy => {
                policy.RequireAuthenticatedUser();
                policy.AuthenticationSchemes.Add("JwtBearer");
            });
        });

        // Add other services
        builder.Services.AddCors();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        WebApplication app = builder.Build();

        // Add a middleware that logs each request to the console
        app.Use(async (HttpContext context, RequestDelegate next) => {
            DateTime now = DateTime.Now;
            await next(context);
            app.Logger.LogInformation($"[{now:yyyy.MM.dd - HH:mm:ss}] {context.Request.Host} → {context.Request.Path} ({context.Response.StatusCode})");
        });

        // Add middlewares for authentication
        app.UseAuthentication();
        app.UseAuthorization();

        // Set CORS to accept any connection
        app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
        
        // Add swagger middlewares when in development mode
        if (app.Environment.IsDevelopment()) {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Map endpoints
        app.MapAuthTestEndpoints();

        // Print the current security key as a base64 string
        app.Logger.LogCritical($"Key: {Convert.ToBase64String(SecurityKey.Key)}");

        app.Run();
    }
}

using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
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
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api;

internal static class Program {
    public static readonly SymmetricSecurityKey SecurityKey = Utils.LoadOrCreateSecurityKey("SECURITY_KEY");

    public static readonly string TokenIssuer = Assembly.GetExecutingAssembly().FullName!;

    public static void Main(string[] args) {
        LdapHelper            ldapHelper = new("localhost", 389, "cn=admin,dc=test,dc=local", "admin") { DnBase = "dc=test,dc=local" };
        WebApplicationBuilder builder    = WebApplication.CreateBuilder(args);

        // Create jwt authentication scheme
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => {
            options.TokenValidationParameters = new TokenValidationParameters {
                ValidateAudience = false,
                ValidateIssuer   = true,
                ValidateLifetime = true,

                IssuerSigningKey = SecurityKey,
                ValidIssuers     = new[] { TokenIssuer },
                ValidAlgorithms  = new[] { SecurityAlgorithms.HmacSha512 }
            };

            options.Events = new JwtBearerEvents {
                // Fail if Authorization header is missing
                OnMessageReceived = (MessageReceivedContext context) => {
                    var headerValues = context.Request.Headers.Authorization;
                    if (headerValues.Count == 0 || headerValues.ToString()[..6].ToLower() != "bearer")
                        context.Fail("missing");

                    return Task.CompletedTask;
                },

                // Send back a new token on successful authentication
                OnTokenValidated = (TokenValidatedContext context) => {
                    AuthenticationHeaderValue header = AuthenticationHeaderValue.Parse(context.Request.Headers.Authorization.ToString());
                    JwtSecurityToken          token  = new JwtSecurityTokenHandler().ReadJwtToken(header.Parameter!);

                    context.Response.Headers.TryAdd("New-Authorization", $"Bearer {AuthHelper.RenewToken(token)}");

                    return Task.CompletedTask;
                },

                // Return 401 from middleware if authentication fails
                OnChallenge = async (JwtBearerChallengeContext context) => {
                    context.HandleResponse();

                    string? error = context.AuthenticateFailure switch {
                        { Message: "missing" } => "Missing json web token.",
                        not null               => "Invalid json web token.",
                        _                      => null
                    };

                    if (error is not null) {
                        context.Response.StatusCode  = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync(context.ErrorDescription.DefaultIfNullOrEmpty(error));
                        await context.Response.CompleteAsync();
                    }
                }
            };
        });

        // Add authorization policy that uses the previously created authentication scheme
        builder.Services.AddAuthorization(options => {
            options.AddPolicy(JwtBearerDefaults.AuthenticationScheme + "Policy", policy => {
                policy.RequireAuthenticatedUser();
                policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
            });
        });

        // Add other services
        builder.Services.AddCors();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSingleton<LdapHelper>(_ => ldapHelper);
        builder.Services.AddSwaggerWrapperGen();

        WebApplication app = builder.Build();
        ldapHelper.Logger = app.Logger;

        // Add a middleware that logs each request to the console
        app.Use(async (HttpContext context, RequestDelegate next) => {
            DateTime now = DateTime.Now;
            await next(context);
            app.Logger.LogInformation($"[{now:yyyy.MM.dd - HH:mm:ss}] {context.Request.Host} → {context.Request.Path} ({context.Response.StatusCode})");
        });

        // Add a middleware for handling LDAP connection binding errors
        app.Use(async (HttpContext context, RequestDelegate next) => {
            try {
                await next(context);
            }
            catch (LdapBindingException) {
                context.Response.StatusCode  = StatusCodes.Status503ServiceUnavailable;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("Unable to connect to the LDAP server.");
                await context.Response.CompleteAsync();
            }
        });

        // Set CORS to accept any connection
        app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

        // Add swagger middlewares when in development mode
        if (app.Environment.IsDevelopment()) {
            app.UseSwaggerWrapper();
        }

        // Map endpoints
        app.MapAuthEndpoints();
        app.MapStudentEndpoints();
        app.MapManagementEndpoints();

        // Print the current security key as a base64 string
        app.Logger.LogCritical($"Key: {Convert.ToBase64String(SecurityKey.Key)}");

        app.Run();
    }
}

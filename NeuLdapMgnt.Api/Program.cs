using System;
using System.DirectoryServices.Protocols;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace NeuLdapMgnt.Api;

internal static class Program {
    public static readonly SymmetricSecurityKey SecurityKey = Utils.LoadOrCreateSecurityKey("SECURITY_KEY");

    public static readonly string TokenIssuer = Assembly.GetExecutingAssembly().FullName!;

    public static void Main(string[] args) {
        LdapHelper ldapHelper = new("localhost", 389, "cn=admin,dc=test,dc=local", "admin") { DnBase = "dc=test,dc=local" };
        ldapHelper.TryRequest(new AddRequest("ou=students,dc=test,dc=local", "organizationalUnit"));
        ldapHelper.TryRequest(new AddRequest("ou=teachers,dc=test,dc=local", "organizationalUnit"));

        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        // Add other services
        builder.Services.AddCors();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton<LdapHelper>(_ => ldapHelper);

        WebApplication app = builder.Build();
        ldapHelper.Logger = app.Logger;

        // Add a middleware that logs each request to the console
        app.Use(async (HttpContext context, RequestDelegate next) => {
            DateTime now = DateTime.Now;
            await next(context);
            app.Logger.LogInformation($"[{now:yyyy.MM.dd - HH:mm:ss}] {context.Request.Host} → {context.Request.Path} ({context.Response.StatusCode})");
        });

        // Set CORS to accept any connection
        app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

        // Add swagger middlewares when in development mode
        if (app.Environment.IsDevelopment()) {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Map endpoints

        // Print the current security key as a base64 string
        app.Logger.LogCritical($"Key: {Convert.ToBase64String(SecurityKey.Key)}");

        app.Run();
    }
}

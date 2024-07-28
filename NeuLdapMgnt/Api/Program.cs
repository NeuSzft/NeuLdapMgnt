using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NeuLdapMgnt.Api.Endpoints;
using NeuLdapMgnt.Api.LdapServiceExtensions;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api;

internal static class Program {
	public static readonly SymmetricSecurityKey SecurityKey = Utils.LoadOrCreateSecurityKey("SECURITY_KEY");

	public static readonly string ServiceName = typeof(Program).Namespace!;

	private static readonly bool CheckHeadersForAddress = Utils.IsEnvTrue("CHECK_HEADERS_FOR_ADDR");

	public static void Main(string[] args) {
		var ldapService = LdapService.FromEnvs();

		ILoggerService loggerService = Utils.IsEnvTrue("LOG_TO_DB")
			? PgLoggerService.FromEnvs().SetIgnoredRoutes("/api/docs")
			: new DummyLoggerService();

		var builder = WebApplication.CreateBuilder(args);

		// Add services
		builder.Services.AddCors();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSingleton(_ => ldapService);
		builder.Services.AddSingleton(_ => loggerService);
		builder.Services.AddSwaggerWrapperGen(ServiceName.ToLower(), "Neu LDAP Management API", Utils.GetAssemblyVersion(typeof(Program).Assembly));

		var app = builder.Build();
		ldapService.Logger = app.Logger;

		// Add a middleware for handling LDAP connection binding errors and other exceptions
		app.Use(async (HttpContext context, RequestDelegate next) => {
			var      now = DateTime.UtcNow;
			LogLevel logLevel;
			string?  note;

			context.Request.Headers.Remove("Neu-Username");
			context.Request.Headers.Remove("Neu-Audience");

			try {
				await next(context);

				logLevel = LogLevel.Information;
				note     = context.Request.Headers.TryGetValue("Neu-Username", out var username) ? $"Log in attempt with the '{username}' user" : null;

				var req = context.Request;
				app.Logger.LogInformation($"[{now:yyyy.MM.dd - HH:mm:ss}] {req.Host} → {req.Method} {req.Path} ({context.Response.StatusCode})");
			}
			catch (LdapBindingException) {
				logLevel                    = LogLevel.Critical;
				note                        = "Unable to connect to the LDAP server";
				context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

				app.Logger.LogCritical($"[{now:yyyy.MM.dd - HH:mm:ss}] {note}");
				await context.RespondWithError(note);
			}
			catch (Exception e) {
				logLevel                    = LogLevel.Error;
				note                        = app.Environment.IsDevelopment() ? e.ToString() : e.Message;
				context.Response.StatusCode = StatusCodes.Status500InternalServerError;

				var req = context.Request;
				app.Logger.LogError($"[{now:yyyy.MM.dd - HH:mm:ss}] {req.Host} → {req.Method} {req.Path} ({context.Response.StatusCode}) | {note}");
				await context.RespondWithError(note);
			}

			context.Request.Headers.TryGetValue("Neu-Audience", out var audience);
			string? fullName = ldapService.TryGetDisplayNameOfEntity(audience.ToString(), typeof(Employee));

			loggerService.CreateLogEntry(new LogEntry {
				Time        = now,
				LogLevel    = logLevel.ToString(),
				Username    = audience.ToString(),
				FullName    = audience == Authenticator.GetDefaultAdminName() ? "DEFAULT ADMIN" : fullName,
				Host        = context.TryGetClientAddress(CheckHeadersForAddress) ?? "unknown",
				Method      = context.Request.Method,
				RequestPath = context.Request.Path,
				StatusCode  = context.Response.StatusCode,
				Note        = note
			});
		});

		// Set CORS to accept any connection
		app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

		// Map endpoints
		app.MapAuthEndpoints();
		app.MapClassEndpoints();
		app.MapStudentEndpoints();
		app.MapEmployeeEndpoints();
		app.MapInactiveUserEndpoints();
		app.MapAdminUserEndpoints();
		app.MapDbManagementEndpoints();
		app.MapLogEndpoints();

		// Map testing endpoints when in development mode
		if (app.Environment.IsDevelopment())
			app.MapTestingEndpoints();

		// Add swagger middlewares
		app.UseSwaggerWrapper(ServiceName.ToLower());

		// Print the current security key as a base64 string when in development mode
		if (app.Environment.IsDevelopment())
			app.Logger.LogDebug("Key: {}", Convert.ToBase64String(SecurityKey.Key));

		app.Run();
	}
}

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

	public static void Main(string[] args) {
		LdapService ldapService = LdapService.FromEnvs();

		ILoggerService loggerService = Utils.IsEnvTrue("LOG_TO_DB")
			? PgLoggerService.FromEnvs().SetIgnoredRoutes("/api/docs")
			: new DummyLoggerService();

		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

		// Create jwt authentication scheme
		builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => {
			options.TokenValidationParameters = new TokenValidationParameters {
				ValidateAudience = false,
				ValidateIssuer   = true,
				ValidateLifetime = true,

				RequireAudience = true,

				IssuerSigningKey = SecurityKey,
				ValidIssuers     = [ ServiceName ],
				ValidAlgorithms  = [ SecurityAlgorithms.HmacSha512 ]
			};

			options.Events = new JwtBearerEvents {
				// Fail if Authorization header is missing
				OnMessageReceived = (MessageReceivedContext context) => {
					var headerValues = context.Request.Headers.Authorization;
					if (headerValues.Count == 0 || !headerValues.ToString().StartsWith("bearer", StringComparison.InvariantCultureIgnoreCase))
						context.Fail("missing");

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
						context.Response.ContentType = MediaTypeNames.Text.Plain;
						context.Response.StatusCode  = StatusCodes.Status401Unauthorized;
						await context.Response.WriteAsync(context.ErrorDescription.FallbackIfNullOrWhitespace(error));
						await context.Response.CompleteAsync();
					}
				},

				// Store the audience of the validated toke it the Audience header
				OnTokenValidated = (TokenValidatedContext context) => {
					JwtSecurityToken token = Authenticator.ReadJwtFromRequestHeader(context.Request);
					context.Request.Headers.Append("Audience", token.Audiences.FirstOrDefault());
					return Task.CompletedTask;
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
		builder.Services.AddSingleton(_ => ldapService);
		builder.Services.AddSingleton(_ => loggerService);
		builder.Services.AddSwaggerWrapperGen(ServiceName.ToLower(), "Neu LDAP Management API", Utils.GetAssemblyVersion(typeof(Program).Assembly));

		WebApplication app = builder.Build();
		ldapService.Logger = app.Logger;

		// Add a middleware that logs each request to the console
		app.Use(async (HttpContext context, RequestDelegate next) => {
			DateTime    now = DateTime.UtcNow;
			HttpRequest req = context.Request;

			await next(context);
			app.Logger.LogInformation($"[{now:yyyy.MM.dd - HH:mm:ss}] {req.Host} → {req.Method} {req.Path} ({context.Response.StatusCode})");

			req.Headers.TryGetValue("Audience", out var aud);
			string user = aud.ToString().FallbackIfNullOrWhitespace("__NOAUTH__");

			loggerService.CreateLogEntry(new() {
				Time        = now,
				LogLevel    = LogLevel.Information.ToString(),
				Username    = aud.ToString(),
				FullName    = aud == Authenticator.GetDefaultAdminName() ? "DEFAULT ADMIN" : ldapService.TryGetDisplayNameOfEntity(user, typeof(Employee)),
				Host        = context.TryGetClientAddress(true) ?? "unknown",
				Method      = req.Method,
				RequestPath = req.Path,
				StatusCode  = context.Response.StatusCode
			});
		});

		// Add a middleware for handling LDAP connection binding errors
		app.Use(async (HttpContext context, RequestDelegate next) => {
			try {
				await next(context);
			}
			catch (LdapBindingException) {
				const string message = "Unable to connect to the LDAP server.";

				context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

				if (context.Request.Headers.Authorization.ToString().StartsWith("bearer", StringComparison.InvariantCultureIgnoreCase)) {
					var result = new RequestResult().SetStatus(StatusCodes.Status503ServiceUnavailable).SetErrors(message).RenewToken(context.Request);
					await context.Response.WriteAsJsonAsync(result);
				}
				else {
					await context.Response.WriteAsync(message);
				}

				await context.Response.CompleteAsync();

				loggerService.CreateLogEntry(new() {
					Time        = DateTime.UtcNow,
					LogLevel    = LogLevel.Critical.ToString(),
					Host        = context.TryGetClientAddress(true) ?? "unknown",
					Method      = context.Request.Method,
					RequestPath = context.Request.Path.ToString(),
					StatusCode  = context.Response.StatusCode
				});
			}
		});

		// Set CORS to accept any connection
		app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

		// Add swagger middlewares
		app.UseSwaggerWrapper(ServiceName.ToLower());

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

		// Print the current security key as a base64 string when in development mode
		if (app.Environment.IsDevelopment())
			app.Logger.LogCritical($"Key: {Convert.ToBase64String(SecurityKey.Key)}");

		app.Run();
	}
}

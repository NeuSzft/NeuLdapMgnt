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

	private static readonly bool CheckHeadersForAddress = Utils.IsEnvTrue("CHECK_HEADERS_FOR_ADDR");

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

		// Add a middleware for handling LDAP connection binding errors and other exceptions
		app.Use(async (HttpContext context, RequestDelegate next) => {
			DateTime now = DateTime.UtcNow;
			LogLevel logLevel;
			string?  note;

			try {
				await next(context);

				logLevel = LogLevel.Information;
				note     = null;

				HttpRequest req = context.Request;
				app.Logger.LogInformation($"[{now:yyyy.MM.dd - HH:mm:ss}] {req.Host} → {req.Method} {req.Path} ({context.Response.StatusCode})");
			}
			catch (LdapBindingException) {
				const string message = "Unable to connect to the LDAP server";

				logLevel                    = LogLevel.Critical;
				note                        = message;
				context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

				app.Logger.LogCritical($"[{now:yyyy.MM.dd - HH:mm:ss}] {message}");
				await context.RespondWithError(message);
			}
			catch (Exception e) {
				logLevel                    = LogLevel.Error;
				note                        = app.Environment.IsDevelopment() ? e.ToString() : e.Message;
				context.Response.StatusCode = StatusCodes.Status500InternalServerError;

				string      message = $"{context.Response.StatusCode}: {note}";
				HttpRequest req     = context.Request;
				app.Logger.LogError($"[{now:yyyy.MM.dd - HH:mm:ss}] {req.Host} → {req.Method} {req.Path} | {message}");
				await context.RespondWithError(message);
			}

			context.Request.Headers.TryGetValue("Audience", out var audience);
			string? fullName = ldapService.TryGetDisplayNameOfEntity(audience.ToString(), typeof(Employee));

			loggerService.CreateLogEntry(new() {
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
			app.Logger.LogDebug("Key: {}", Convert.ToBase64String(SecurityKey.Key));

		app.Run();
	}
}

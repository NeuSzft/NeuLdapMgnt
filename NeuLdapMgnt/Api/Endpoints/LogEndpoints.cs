using System;
using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class LogEndpoints {
	public static void MapLogEndpoints(this IEndpointRouteBuilder app) {
		app.MapGet("/api/logs", (ILoggerService loggerService, HttpRequest request, string? from, string? to) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				if (loggerService is DummyLoggerService)
					return new RequestResult().SetStatus(StatusCodes.Status503ServiceUnavailable).SetErrors("Logging to database is disabled.").RenewToken(request).ToResult();

				var fromOffset = long.TryParse(from, out long fromSecs)
					? DateTimeOffset.FromUnixTimeSeconds(fromSecs)
					: DateTimeOffset.UnixEpoch;

				var toOffset = long.TryParse(to, out long toSecs)
					? DateTimeOffset.FromUnixTimeSeconds(toSecs)
					: DateTimeOffset.UtcNow;

				return new RequestResult<string>().SetValues(loggerService.GetLogEntries(fromOffset.DateTime, toOffset.DateTime).Select(LogEntry.ToTsv).ToArray()).RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Logs")
			.WithDescription(
				"""
				### Returns all request logs within the specified timeframe.

				The timeframe can be specified with the "**from**" and "**to**" URL parameters which use Unix time.\
				Both parameters are optional, and the default values are *1970.01.01* for "**from**" and the *current time* for "**to**" meaning that all logs are returned.

				*Unix time is the seconds passed since 1970.01.01 00:00:00 (UTC).*
				"""
			)
			.Produces<RequestResult<string>>()
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
	}
}

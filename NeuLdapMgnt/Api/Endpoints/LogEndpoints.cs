using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class LogEndpoints {
	public static void MapLogEndpoints(this WebApplication app) {
		app.MapGet("/api/logs", (PostgresService pg, HttpRequest request) => {
			   DateTimeOffset from = request.Query.TryGetValue("from", out var fromStr) && long.TryParse(fromStr, out var fromSecs)
				   ? DateTimeOffset.FromUnixTimeSeconds(fromSecs)
				   : DateTimeOffset.UnixEpoch;

			   DateTimeOffset to = request.Query.TryGetValue("to", out var toStr) && long.TryParse(toStr, out var toSecs)
				   ? DateTimeOffset.FromUnixTimeSeconds(toSecs)
				   : DateTimeOffset.UtcNow;

			   return new RequestResult<LogEntry>().SetValues(pg.GetLogEntries(from.DateTime, to.DateTime).ToArray()).RenewToken(request);
		   })
		   .WithOpenApi()
		   .WithTags("Logs")
		   .RequireAuthorization()
		   .Produces<RequestResult<LogEntry>>()
		   .Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
		   .Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
	}
}

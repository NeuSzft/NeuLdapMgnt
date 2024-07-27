using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NeuLdapMgnt.Models;
using Npgsql;

namespace NeuLdapMgnt.Api;

/// <summary>Represents a type used for logging incoming API requests.</summary>
public interface ILoggerService {
	/// <summary>Creates a new log based on the provided <paramref name="entry"/>.</summary>
	/// <param name="entry">The <see cref="LogEntry"/> to log.</param>
	void CreateLogEntry(LogEntry entry);

	/// <summary>Returns the log entries that were created during the specified time period.</summary>
	/// <param name="start">The beginning of the time period.</param>
	/// <param name="end">The end of the time period.</param>
	/// <returns>The entries as <see cref="LogEntry"/> objects.</returns>
	IEnumerable<LogEntry> GetLogEntries(DateTime start, DateTime end);
}

/// <summary>A dummy <see cref="ILoggerService"/> that does not perform any logging.</summary>
public sealed class DummyLoggerService : ILoggerService {
	/// <summary>Takes in a <see cref="LogEntry"/> but does nothing.</summary>
	public void CreateLogEntry(LogEntry entry) { }

	/// <summary>Takes in two timestamps and returns an empty collection.</summary>
	public IEnumerable<LogEntry> GetLogEntries(DateTime start, DateTime end) => [ ];
}

/// <summary>An <see cref="ILoggerService"/> that logs into a Postgres database.</summary>
public sealed class PgLoggerService : ILoggerService {
	private readonly string _connectionString;

	private IEnumerable<string> _ignoredRoutes = [ ];

	/// <param name="host">The host to connect to.</param>
	/// <param name="db">The name of the database to use.</param>
	/// <param name="password">The password of the "postgres" user.</param>
	public PgLoggerService(string host, string db, string password) {
		_connectionString = $"Host={host};Database={db};Username=postgres;Password={password}";
		CreateDefaultTables();
	}

	/// <summary>Creates a new <see cref="PgLoggerService"/> using the specified environment variables.</summary>
	/// <param name="hostEnv">The environment variable that specifies the host of the Postgres database.</param>
	/// <param name="dbEnv">The environment variable that specifies the name of the database.</param>
	/// <param name="passwordEnv">The environment variable that specifies the password of the "postgres" user.</param>
	/// <returns>The new <see cref="PgLoggerService"/>.</returns>
	/// <exception cref="ArgumentException">An environment variable is not set or is an empty string.</exception>
	public static PgLoggerService FromEnvs(string hostEnv = "POSTGRES_HOST", string dbEnv = "POSTGRES_DB", string passwordEnv = "POSTGRES_PASSWORD") {
		return new(
			Utils.GetEnv(hostEnv),
			Utils.GetEnv(dbEnv),
			Utils.GetEnv(passwordEnv)
		);
	}

	/// <summary>Opens a new <see cref="NpgsqlConnection"/>.</summary>
	/// <returns>The opened <see cref="NpgsqlConnection"/>.</returns>
	private NpgsqlConnection OpenConnection() {
		NpgsqlConnection connection = new(_connectionString);
		connection.Open();
		return connection;
	}

	/// <summary>Specifies the API routes whose requests should not be logged.</summary>
	/// <param name="ignoredRoutes">The routes that should be ignored when logging.</param>
	/// <returns>This <see cref="PgLoggerService"/>.</returns>
	public PgLoggerService SetIgnoredRoutes(params string[] ignoredRoutes) {
		_ignoredRoutes = ignoredRoutes;
		return this;
	}

	public void CreateLogEntry(LogEntry entry) {
		if (_ignoredRoutes.Any(entry.RequestPath.StartsWith))
			return;

		string query = string.IsNullOrEmpty(entry.Username) || string.IsNullOrEmpty(entry.FullName)
			? """
			INSERT INTO "entries"
			("time", "log_level", "username", "host", "method", "request_path", "status_code", "note")
			VALUES
			(@Time, @LogLevel, null, @Host, @Method, @RequestPath, @StatusCode, @Note);
			"""
			: """
			INSERT INTO "users"
			("username", "full_name")
			VALUES
			(@Username, @FullName)
			ON CONFLICT DO NOTHING;

			INSERT INTO "entries"
			("time", "log_level", "username", "host", "method", "request_path", "status_code", "note")
			VALUES
			(@Time, @LogLevel, @Username, @Host, @Method, @RequestPath, @StatusCode, @Note);
			""";

		using NpgsqlConnection connection = OpenConnection();
		connection.Execute(query, new {
			entry.Time,
			entry.LogLevel,
			entry.Username,
			entry.FullName,
			entry.Host,
			entry.Method,
			entry.RequestPath,
			entry.StatusCode,
			entry.Note
		});
	}

	/// <summary>Returns the logs made in the specified timeframe from the "entries" table.</summary>
	public IEnumerable<LogEntry> GetLogEntries(DateTime start, DateTime end) {
		const string query = """
			SELECT
				entries.id AS "Id",
				entries.time AS "Time",
				entries.log_level AS "LogLevel",
				entries.username AS "UserName",
				users.full_name AS "FullName",
				entries.host AS "Host",
				entries.method AS "Method",
				entries.request_path AS "RequestPath",
				entries.status_code AS "StatusCode",
				entries.note AS "Note"
			FROM entries
			LEFT JOIN users ON entries.username = users.username
			WHERE time BETWEEN @Start AND @End
			ORDER BY entries.id;
			""";

		using NpgsqlConnection connection = OpenConnection();
		return connection.Query<LogEntry>(query, new { Start = start, End = end });
	}

	/// <summary>Creates the "users" and "entries" tables within the database.</summary>
	private void CreateDefaultTables() {
		const string query = """
			CREATE TABLE IF NOT EXISTS users(
				username VARCHAR(255) PRIMARY KEY,
				full_name VARCHAR(255)
			);

			CREATE TABLE IF NOT EXISTS entries(
				id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
				time TIMESTAMP NOT NULL,
				log_level VARCHAR(11) NOT NULL,
				username VARCHAR(255) REFERENCES users(username),
				host VARCHAR(255) NOT NULL,
				method VARCHAR(7) NOT NULL,
				request_path VARCHAR(255) NOT NULL,
				status_code INT NOT NULL,
				note VARCHAR(255)
			);
			""";

		using NpgsqlConnection connection = OpenConnection();
		connection.Query(query);
	}
}

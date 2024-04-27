using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NeuLdapMgnt.Models;
using Npgsql;

namespace NeuLdapMgnt.Api;

public interface ILoggerService {
	void CreateLogEntry(LogEntry entry);

	IEnumerable<LogEntry> GetLogEntries(DateTime start, DateTime end);
}

public sealed class DummyLoggerService : ILoggerService {
	public void CreateLogEntry(LogEntry entry) { }

	public IEnumerable<LogEntry> GetLogEntries(DateTime start, DateTime end) => [];
}

public sealed class PglLoggerService : ILoggerService {
	private readonly string _connectionString;

	private IEnumerable<string> _ignoredRoutes = [];

	public PglLoggerService(string host, string db, string password) {
		_connectionString = $"Host={host};Database={db};Username=postgres;Password={password}";
		CreateDefaultTables();
	}

	public static PglLoggerService FromEnvs(string hostEnv = "POSTGRES_HOST", string dbEnv = "POSTGRES_DB", string passwordEnv = "POSTGRES_PASSWORD") {
		return new(
			Environment.GetEnvironmentVariable(hostEnv).ThrowIfNullOrEmpty(),
			Environment.GetEnvironmentVariable(dbEnv).ThrowIfNullOrEmpty(),
			Environment.GetEnvironmentVariable(passwordEnv).ThrowIfNullOrEmpty()
		);
	}

	private NpgsqlConnection OpenConnection() {
		NpgsqlConnection connection = new(_connectionString);
		connection.Open();
		return connection;
	}

	public PglLoggerService SetIgnoredRoutes(params string[] ignoredRoutes) {
		_ignoredRoutes = ignoredRoutes;
		return this;
	}

	public void CreateLogEntry(LogEntry entry) {
		if (_ignoredRoutes.Any(entry.RequestPath.StartsWith))
			return;

		string query = string.IsNullOrEmpty(entry.Username) || string.IsNullOrEmpty(entry.FullName)
			? """
				INSERT INTO "entries"
				("time", "log_level", "username", "host", "method", "request_path", "status_code")
				VALUES
				(@Time, @LogLevel, null, @Host, @Method, @RequestPath, @StatusCode);
			"""
			: """
				INSERT INTO "users"
				("username", "full_name")
				VALUES
				(@Username, @FullName)
				ON CONFLICT DO NOTHING;
				
				INSERT INTO "entries"
				("time", "log_level", "username", "host", "method", "request_path", "status_code")
				VALUES
				(@Time, @LogLevel, @Username, @Host, @Method, @RequestPath, @StatusCode);
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
			entry.StatusCode
		});
	}

	public IEnumerable<LogEntry> GetLogEntries(DateTime start, DateTime end) {
		string query = """
			SELECT
			    entries.id AS "Id",
				entries.time AS "Time",
				entries.log_level AS "LogLevel",
				entries.username AS "UserName",
				users.full_name AS "FullName",
				entries.host AS "Host",
				entries.method AS "Method",
				entries.request_path AS "RequestPath",
				entries.status_code AS "StatusCode"
			FROM entries
			LEFT JOIN users ON entries.username = users.username
			WHERE time BETWEEN @Start AND @End;
			""";

		using NpgsqlConnection connection = OpenConnection();
		return connection.Query<LogEntry>(query, new { Start = start, End = end });
	}

	private void CreateDefaultTables() {
		string query = """
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
				status_code INT NOT NULL
			);
			""";

		using NpgsqlConnection connection = OpenConnection();
		connection.Query(query);
	}
}

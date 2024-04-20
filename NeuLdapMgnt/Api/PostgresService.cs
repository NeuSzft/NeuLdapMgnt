using System;
using System.Collections.Generic;
using Dapper;
using NeuLdapMgnt.Models;
using Npgsql;

namespace NeuLdapMgnt.Api {
	public sealed class PostgresService {
		private readonly NpgsqlConnection _connection;

		public PostgresService(string host, string db, string password) {
			_connection = new($"Host={host};Database={db};Username=postgres;Password={password}");
			CreateDefaultTables();
		}

		public static PostgresService FromEnvs(string hostEnv = "POSTGRES_HOST", string dbEnv = "POSTGRES_DB", string passwordEnv = "POSTGRES_PASSWORD") {
			return new(
				Environment.GetEnvironmentVariable(hostEnv).ThrowIfNullOrEmpty(),
				Environment.GetEnvironmentVariable(dbEnv).ThrowIfNullOrEmpty(),
				Environment.GetEnvironmentVariable(passwordEnv).ThrowIfNullOrEmpty()
			);
		}

		public void CreateLogEntry(LogEntry entry) {
			string query;

			if (string.IsNullOrEmpty(entry.Username) || string.IsNullOrEmpty(entry.FullName)) {
				query = """
					INSERT INTO "entries"
					("time", "log_level", "username", "host", "method", "request_path", "status_code")
					VALUES
					(@Time, @LogLevel, null, @Host, @Method, @RequestPath, @StatusCode);
				""";
			}
			else {
				query = """
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
			}

			_connection.Open();
			_connection.Execute(query, new {
				entry.Time,
				entry.LogLevel,
				entry.Username,
				entry.FullName,
				entry.Host,
				entry.Method,
				entry.RequestPath,
				entry.StatusCode
			});
			Console.WriteLine(entry);
			_connection.Close();
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

			_connection.Open();
			var entries = _connection.Query<LogEntry>(query, new { Start = start, End = end });
			_connection.Close();
			return entries;
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

			_connection.Open();
			_connection.Query(query);
			_connection.Close();
		}
	}
}

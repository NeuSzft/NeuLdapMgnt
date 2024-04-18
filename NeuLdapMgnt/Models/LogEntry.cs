using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace NeuLdapMgnt.Models
{
	public class LogEntry
	{
		[JsonPropertyName("id")]
		public long Id { get; set; }

		[JsonPropertyName("time")]
		public required DateTime Time { get; set; }

		[JsonPropertyName("log_level")]
		public required string LogLevel { get; set; }

		[JsonPropertyName("username")]
		public string? Username { get; set; }

		[JsonPropertyName("full_name")]
		public string? FullName { get; set; }

		[JsonPropertyName("host")]
		public required string Host { get; set; }

		[JsonPropertyName("method")]
		public required string Method { get; set; }

		[JsonPropertyName("request_path")]
		public required string RequestPath { get; set; }

		[JsonPropertyName("status_code")]
		public required int StatusCode { get; set; }

		public override string ToString()
		{
			return $"[{Time:yyyy.MM.dd - HH:mm:ss}] {Host} → {Method} {RequestPath} ({StatusCode})";
		}
	}
}

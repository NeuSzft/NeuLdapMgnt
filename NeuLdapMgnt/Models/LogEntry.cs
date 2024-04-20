using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeuLdapMgnt.Models;

public class LogEntryIdConverter : JsonConverter<BigInteger> {
	public override BigInteger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetString() is { } value ? BigInteger.Parse(value) : 0;

	public override void Write(Utf8JsonWriter writer, BigInteger value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}

public class LogEntry {
	[JsonRequired, JsonPropertyName("id"), JsonConverter(typeof(LogEntryIdConverter))]
	public BigInteger Id { get; init; }

	[JsonRequired, JsonPropertyName("time")]
	public required DateTime Time { get; init; }

	[JsonRequired, JsonPropertyName("log_level")]
	public required string LogLevel { get; init; }

	[JsonInclude, JsonPropertyName("username")]
	public string? Username { get; init; }

	[JsonInclude, JsonPropertyName("full_name")]
	public string? FullName { get; init; }

	[JsonRequired, JsonPropertyName("host")]
	public required string Host { get; init; }

	[JsonRequired, JsonPropertyName("method")]
	public required string Method { get; init; }

	[JsonRequired, JsonPropertyName("request_path")]
	public required string RequestPath { get; init; }

	[JsonRequired, JsonPropertyName("status_code")]
	public required int StatusCode { get; init; }

	public override string ToString() {
		return $"[{Time:yyyy.MM.dd - HH:mm:ss}] {Host} → {Method} {RequestPath} ({StatusCode})";
	}
}

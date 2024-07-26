using System;
using System.Globalization;
using System.Numerics;

namespace NeuLdapMgnt.Models;

public class LogEntry {
	public BigInteger Id { get; init; }

	public required DateTime Time { get; init; }

	public required string LogLevel { get; init; }

	public string? Username { get; init; }

	public string? FullName { get; init; }

	public required string Host { get; init; }

	public required string Method { get; init; }

	public required string RequestPath { get; init; }

	public required int StatusCode { get; init; }

	public string? Note { get; init; }

	public override string ToString() {
		return $"[{Time:yyyy.MM.dd - HH:mm:ss}] {Host} → {Method} {RequestPath} ({StatusCode})";
	}

	private static readonly CultureInfo CultureInfo = CultureInfo.GetCultureInfoByIetfLanguageTag("hu-HU");

	/// <summary>Creates a tab separated vales (tsv) string from a <see cref="LogEntry"/>.</summary>
	/// <param name="entry">The <see cref="LogEntry"/> to use.</param>
	/// <returns>The values of the <see cref="LogEntry"/> separated by tabs.</returns>
	public static string ToTsv(LogEntry entry) {
		return $"{entry.Id}\t{entry.Time.ToString(CultureInfo)}\t{entry.LogLevel}\t{entry.Username}\t{entry.FullName}\t{entry.Host}\t{entry.Method}\t{entry.RequestPath}\t{entry.StatusCode}\t{entry.Note}";
	}

	/// <summary>Creates a <see cref="LogEntry"/> from tab separated vales (tsv).</summary>
	/// <param name="tsvLine">A line of string containing the values of the <see cref="LogEntry"/> separated by tabs.</param>
	/// <returns>The newly created <see cref="LogEntry"/>.</returns>
	/// <exception cref="FormatException">The format of the string is invalid.</exception>
	public static LogEntry FromTsv(string tsvLine) {
		string[] values = tsvLine.Split('\t');
		try {
			return new() {
				Id          = BigInteger.Parse(values[0]),
				Time        = DateTime.Parse(values[1], CultureInfo),
				LogLevel    = values[2],
				Username    = string.IsNullOrEmpty(values[3]) ? null : values[3],
				FullName    = string.IsNullOrEmpty(values[4]) ? null : values[4],
				Host        = values[5],
				Method      = values[6],
				RequestPath = values[7],
				StatusCode  = int.Parse(values[8]),
				Note        = values[9]
			};
		}
		catch {
			throw new FormatException($"The tab separated line cannot be converted into a {typeof(LogEntry)} using the {CultureInfo} culture");
		}
	}
}

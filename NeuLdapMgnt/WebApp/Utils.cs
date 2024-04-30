using System.Net;
using System.Text.Json;

namespace NeuLdapMgnt.WebApp;

/// <summary>
/// Utility methods for various operations within the web application.
/// </summary>
public static class Utils
{
	/// <summary>
	/// Creates a deep copy of the provided object using JSON serialization and deserialization.
	/// </summary>
	/// <typeparam name="T">Type of the object to clone.</typeparam>
	/// <param name="obj">Object to clone.</param>
	/// <returns>A deep copy of the input object.</returns>
	public static T? GetClone<T>(T obj)
	{
		if (obj is null) return default;

		string objJson = JsonSerializer.Serialize<T>(obj);
		return JsonSerializer.Deserialize<T>(objJson);
	}

	/// <summary>
	/// Retrieves an error message corresponding to the provided HTTP request exception.
	/// </summary>
	/// <param name="httpError">The HTTP request exception.</param>
	/// <returns>An error message describing the HTTP error.</returns>
	public static string GetErrorMessage(this HttpRequestException httpError)
	{
		if (httpError.StatusCode.HasValue)
		{
			return httpError.StatusCode.Value switch
			{
				HttpStatusCode.BadRequest          => "Invalid request. Check your input.",
				HttpStatusCode.Unauthorized        => "You are not authorized.",
				HttpStatusCode.Forbidden           => "Access denied.",
				HttpStatusCode.NotFound            => "Resource was not found.",
				HttpStatusCode.Conflict            => "There was a conflict.",
				HttpStatusCode.InternalServerError => "Internal server error. Try again later.",
				HttpStatusCode.ServiceUnavailable  => "The service is unavailable. Try again later.",
				_                                  => $"HTTP Error {httpError.StatusCode}: {httpError.Message}",
			};
		}

		return httpError.Message.Contains("failed to fetch", StringComparison.OrdinalIgnoreCase)
			? "Failed to connect to the server."
			: $"An error occurred: {httpError.Message}";
	}

	/// <summary>
	/// Retrieves the order value of a class based on its name.
	/// </summary>
	/// <param name="cls">The name of the class.</param>
	/// <returns>The order value of the class.</returns>
	public static int GetClassOrderValue(string cls)
	{
		try
		{
			int value = 0;

			if (cls.Contains('/'))
			{
				string[] split = cls.Split('/');
				if (int.TryParse(split[0], out var num))
					value |= num << 16;
				if (int.TryParse(new string(split[1].Where(char.IsDigit).ToArray()), out num))
					value |= num << 8;
				value |= cls.Last();
			}
			else
			{
				string[] split = cls.Split('.');
				if (int.TryParse(split[0], out var num))
					value |= num << 8;
				char c = split[1].ToUpper().First();
				if (c != 'N')
					value |= c;
			}

			return value;
		}
		catch
		{
			return int.MinValue;
		}
	}

	/// <summary>
	/// Checks if the provided ID belongs to a student.
	/// </summary>
	/// <param name="id">The ID to check.</param>
	/// <returns>True if the ID belongs to a student, otherwise false.</returns>
	public static bool IsStudent(string id) => long.TryParse(id, out long studentId) && studentId != 0;
}

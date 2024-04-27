using System.Net;
using System.Text.Json;

namespace NeuLdapMgnt.WebApp
{
	public static class Utils
	{
		public static T? GetClone<T>(T obj)
		{
			if (obj is null) return default;

			string objJson = JsonSerializer.Serialize<T>(obj);
			return JsonSerializer.Deserialize<T>(objJson);
		}

		public static string GetErrorMessage(this HttpRequestException httpError)
		{
			if (httpError.StatusCode.HasValue)
			{
				return httpError.StatusCode.Value switch
				{
					HttpStatusCode.BadRequest => "Invalid request. Check your input.",
					HttpStatusCode.Unauthorized => "You are not authorized.",
					HttpStatusCode.Forbidden => "Access denied.",
					HttpStatusCode.NotFound => "Resource was not found.",
					HttpStatusCode.Conflict => "There was a conflict.",
					HttpStatusCode.InternalServerError => "Internal server error. Try again later.",
					HttpStatusCode.ServiceUnavailable => "The service is unavailable. Try again later.",
					_ => $"HTTP Error {httpError.StatusCode}: {httpError.Message}",
				};
			}
			else
			{
				if (httpError.Message.Contains("failed to fetch", StringComparison.OrdinalIgnoreCase))
				{
					return "Failed to connect to the server.";
				}
				else
				{
					return $"An error occurred: {httpError.Message}";
				}
			}
		}

		public static int GetClassOrderValue(string cls) {
			try {
				int value = 0;

				if (cls.Contains('/')) {
					string[] split = cls.Split('/');
					if (int.TryParse(split[0], out var num))
						value |= num << 16;
					if (int.TryParse(new string(split[1].Where(char.IsDigit).ToArray()), out num))
						value |= num << 8;
					value |= cls.Last();
				}
				else {
					string[] split = cls.Split('.');
					if (int.TryParse(split[0], out var num))
						value |= num << 8;
					char c = split[1].ToUpper().First();
					if (c != 'N')
						value |= c;
				}

				return value;
			}
			catch {
				return int.MinValue;
			}
		}
	}
}

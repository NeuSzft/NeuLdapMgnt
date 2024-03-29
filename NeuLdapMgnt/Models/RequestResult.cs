using System.Linq;
using System.Text.Json.Serialization;

namespace NeuLdapMgnt.Models;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(RequestResult))]
public class RequestResult {
    [JsonRequired, JsonInclude, JsonPropertyName("status_code")]
    public int StatusCode { get; protected internal set; } = 200;

    [JsonRequired, JsonInclude, JsonPropertyName("errors")]
    public string[] Errors { get; protected internal set; } = new string[0];

    [JsonInclude, JsonPropertyName("new_token")]
    public string? NewToken { get; protected internal set; }


    public string GetError() => string.Join('\n', Errors);

    public bool IsSuccess() => StatusCode is >= 200 and <= 299;

    public bool IsFailure() => !IsSuccess();
}

public sealed class RequestResult<T> : RequestResult {
    [JsonRequired, JsonInclude, JsonPropertyName("values")]
    public T[] Values { get; internal set; } = new T[0];


    public T? GetValue() => Values.FirstOrDefault();
}

public static class RequestResultExtensions {
    public static R SetStatus<R>(this R result, int statusCode) where R : RequestResult {
        result.StatusCode = statusCode;
        return result;
    }

    public static R SetErrors<R>(this R result, params string[] errors) where R : RequestResult {
        result.Errors = errors;
        return result;
    }

    public static R SetToken<R>(this R result, string base64UrlEncodedToken) where R : RequestResult {
        result.NewToken = base64UrlEncodedToken;
        return result;
    }

    public static RequestResult<T> SetValues<T>(this RequestResult<T> result, params T[] values) {
        result.Values = values;
        return result;
    }
}

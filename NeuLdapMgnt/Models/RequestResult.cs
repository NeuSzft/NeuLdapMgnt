using System.Linq;
using System.Text.Json.Serialization;

namespace NeuLdapMgnt.Models;

/// <summary>A model for storing the outcome of HTTP requests.</summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(RequestResult))]
public class RequestResult {
    /// <summary>The status code of the response.</summary>
    [JsonRequired, JsonInclude, JsonPropertyName("status_code")]
    public int StatusCode { get; protected internal set; } = 200;

    /// <summary>The errors that occured.</summary>
    [JsonRequired, JsonInclude, JsonPropertyName("errors")]
    public string[] Errors { get; protected internal set; } = new string[0];

    /// <summary>The renewed JSON Web Token to use for the next request.</summary>
    [JsonInclude, JsonPropertyName("new_token")]
    public string? NewToken { get; protected internal set; }

    /// <summary>Gets the error message of the response.</summary>
    /// <returns>The <c>Errors</c> joined together by new lines.</returns>
    public string GetError() => string.Join('\n', Errors);

    /// <summary>Checks if the response is successful or not.</summary>
    /// <returns><c>true</c> if <c>StatusCode</c> is between 200 and 299 and there are no errors, otherwise <c>false</c>.</returns>
    public bool IsSuccess() => StatusCode is >= 200 and <= 299 && Errors.Length == 0;

    /// <summary>Checks if the response is a failure or not.</summary>
    /// <returns><c>false</c> if <c>StatusCode</c> is between 200 and 299 and there are no errors, otherwise <c>true</c>.</returns>
    public bool IsFailure() => !IsSuccess();
}

/// <summary>A model for storing the outcome of HTTP requests.</summary>
/// <typeparam name="T">The type of the values to store.</typeparam>
public sealed class RequestResult<T> : RequestResult {
    [JsonRequired, JsonInclude, JsonPropertyName("values")]
    public T[] Values { get; internal set; } = new T[0];

    /// <summary>Gets the first or the default value.</summary>
    /// <returns>The first element of <c>Values</c> or the <c>default</c> value of type <typeparamref name="T"/>.</returns>
    public T? GetValue() => Values.FirstOrDefault();
}

public static class RequestResultExtensions {
    /// <summary>Sets the <c>StatusCode</c> of a <see cref="RequestResult"/>.</summary>
    /// <param name="result">The <see cref="RequestResult"/> to set the status code of.</param>
    /// <param name="statusCode">The status code to set <c>StatusCode</c> to.</param>
    /// <typeparam name="R">The type of the result which must inherit from <see cref="RequestResult"/>.</typeparam>
    /// <returns>The original result object.</returns>
    public static R SetStatus<R>(this R result, int statusCode) where R : RequestResult {
        result.StatusCode = statusCode;
        return result;
    }

    /// <summary>Sets the <c>Errors</c> of a <see cref="RequestResult"/>.</summary>
    /// <param name="result">The <see cref="RequestResult"/> to set the errors of.</param>
    /// <param name="errors">The error messages to set <c>Errors</c> to.</param>
    /// <typeparam name="R">The type of the result which must inherit from <see cref="RequestResult"/>.</typeparam>
    /// <returns>The original result object.</returns>
    public static R SetErrors<R>(this R result, params string[] errors) where R : RequestResult {
        result.Errors = errors;
        return result;
    }

    /// <summary>Sets the <c>NewToken</c> of a <see cref="RequestResult"/>.</summary>
    /// <param name="result">The <see cref="RequestResult"/> to set the status code of.</param>
    /// <param name="base64UrlEncodedToken">The Base64Url encoded JSON Web Token to set <c>NewToken</c> to.</param>
    /// <typeparam name="R">The type of the result which must inherit from <see cref="RequestResult"/>.</typeparam>
    /// <returns>The original result object.</returns>
    public static R SetToken<R>(this R result, string base64UrlEncodedToken) where R : RequestResult {
        result.NewToken = base64UrlEncodedToken;
        return result;
    }

    /// <summary>Sets the <c>Values</c> of a <see cref="RequestResult{T}"/>.</summary>
    /// <param name="result">The <see cref="RequestResult{T}"/> to set the values code of.</param>
    /// <param name="values">The values to set <c>Values</c> to.</param>
    /// <typeparam name="T">The type of the <paramref name="values"/>.</typeparam>
    /// <returns>The original result object.</returns>
    public static RequestResult<T> SetValues<T>(this RequestResult<T> result, params T[] values) {
        result.Values = values;
        return result;
    }
}

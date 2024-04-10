using System.Text;
using System.Text.Json;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Tests;

public static class Testing {
    public static readonly HttpClient Client = new() { BaseAddress = new(Environment.GetEnvironmentVariable("API_URL")!) };
}

public static class Extensions {
    public static HttpRequestMessage SetAuthHeader(this HttpRequestMessage request, string scheme, string value) {
        request.Headers.Authorization = new(scheme, value);
        return request;
    }

    public static HttpRequestMessage AuthWithBasic(this HttpRequestMessage request, string user, string password) {
        byte[] bytes = Encoding.UTF8.GetBytes($"{user}:{password}");
        string value = Convert.ToBase64String(bytes);
        return request.SetAuthHeader("Basic", value);
    }

    public static HttpRequestMessage AuthWithJwt(this HttpRequestMessage request) {
        string token = Testing.Client.GetStringAsync("/api/testing/get-token").Result;
        return request.SetAuthHeader("Bearer", token);
    }

    public static HttpRequestMessage AuthWithExpiredJwt(this HttpRequestMessage request) {
        string token = Testing.Client.GetStringAsync("/api/testing/get-exp-token").Result;
        return request.SetAuthHeader("Bearer", token);
    }

    public static bool CheckStatusCode(this HttpResponseMessage response, int code) {
        using Stream   stream = response.Content.ReadAsStream();
        RequestResult? result = JsonSerializer.Deserialize<RequestResult>(stream);
        return (int)response.StatusCode == code && result is not null && result.StatusCode == code;
    }
}

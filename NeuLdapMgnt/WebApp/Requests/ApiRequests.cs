using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using NeuLdapMgnt.Models;
using NeuLdapMgnt.WebApp.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace NeuLdapMgnt.WebApp.Requests;

/// <summary>
/// Class responsible for making API requests.
/// </summary>
public class ApiRequests
{
	[Inject] public JwtService JwtService { get; set; }

	private readonly HttpClient        _httpClient;
	public           JwtSecurityToken? CurrentToken { get; private set; }

	public event Action? AuthenticationStateChanged;

	/// <summary>
	/// Property to check if a user token exists, indicating the user is authenticated.
	/// </summary>
	public bool IsAuthenticated => CurrentToken != null;

	/// <summary>
	/// Constructor for ApiRequests.
	/// </summary>
	/// <param name="baseUri">The base URI for the API requests.</param>
	/// <param name="jwtService">Instance of JwtService for decoding JWT tokens.</param>
	public ApiRequests(string baseUri, JwtService jwtService)
	{
		_httpClient = new()
		{
			BaseAddress = new(baseUri),
			DefaultRequestHeaders = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } }
		};
		JwtService = jwtService;
	}

	/// <summary>
	/// Redirects to login if the user is not authenticated.
	/// </summary>
	/// <param name="navManager">The NavigationManager for navigating to login page.</param>
	/// <returns>True if the user is authenticated, otherwise false.</returns>
	public bool EnsureAuthentication(NavigationManager navManager)
	{
		if (!IsAuthenticated)
			navManager.NavigateTo("login");
		return IsAuthenticated;
	}

	/// <summary>
	/// Throws an exception if the user is not authenticated.
	/// </summary>
	private void EnsureAuthentication()
	{
		if (!IsAuthenticated)
		{
			throw new InvalidOperationException("User is not authenticated.");
		}
	}

	/// <summary>
	/// Performs login operation by sending credentials to the server.
	/// </summary>
	/// <param name="username">The username for login.</param>
	/// <param name="password">The password for login.</param>
	/// <returns>True if login is successful, otherwise false.</returns>
	public async Task<bool> LoginAsync(string username, string password)
	{
		string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));
		_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

		var response = await _httpClient.GetAsync("api/auth");
		if (!response.IsSuccessStatusCode) return false;

		string token = await response.Content.ReadAsStringAsync();
		_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
		CurrentToken = JwtService.DecodeToken(token);
		AuthenticationStateChanged?.Invoke();
		return true;
	}

	/// <summary>
	/// Logs out the current user by clearing the authentication token.
	/// </summary>
	public void Logout()
	{
		CurrentToken = null;
		AuthenticationStateChanged?.Invoke();
	}

	/// <summary>
	/// Updates the authentication token based on the server response.
	/// </summary>
	/// <param name="result">The result of the server response.</param>
	private void UpdateToken(RequestResult? result)
	{
		if (result is null || string.IsNullOrEmpty(result.NewToken))
		{
			throw new InvalidOperationException("New token is missing.");
		}

		CurrentToken = JwtService.DecodeToken(result.NewToken);
		_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.NewToken);
	}

	/// <summary>Sends a request to the specified URI with the string content in its body, when the <paramref name="method"/> is either <c>POST</c> or <c>PUT</c>.</summary>
	public async Task<RequestResult> SendStringAsync(HttpMethod method, string uri, string? content)
	{
		EnsureAuthentication();

		HttpRequestMessage request = new(method, uri);

		if (content != null && (method == HttpMethod.Post || method == HttpMethod.Put))
			request.Content = new StringContent(content);

		HttpResponseMessage response = await _httpClient.SendAsync(request);

		RequestResult? result = await response.Content.ReadFromJsonAsync<RequestResult>();
		if (result is not null)
			UpdateToken(result);
		return result ?? new RequestResult().SetStatus(204).SetErrors("Invalid response from server.");
	}

	// Sends a request to the specified URI with optional content, processing HTTP methods accordingly
	public async Task<RequestResult<T>> SendRequestAsync<T>(HttpMethod method, string uri, object? content = null)
	{
		EnsureAuthentication();

		HttpRequestMessage request = new(method, uri);

		if (content != null && (method == HttpMethod.Post || method == HttpMethod.Put))
		{
			request.Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
		}

		var response = await _httpClient.SendAsync(request);
		return await ProcessResponse<T>(response, method);
	}

	/// <summary>
	/// Sends a request to the specified URI with optional content, processing HTTP methods accordingly.
	/// </summary>
	/// <typeparam name="T">The type of the expected response.</typeparam>
	/// <param name="method">The HTTP method of the request.</param>
	/// <param name="uri">The URI of the request.</param>
	/// <param name="content">The content of the request.</param>
	/// <returns>The result of the request.</returns>
	public async Task<RequestResult> SendRequestAsync(HttpMethod method, string uri, object? content = null)
	{
		EnsureAuthentication();

		HttpRequestMessage request = new(method, uri);

		// Sets the request content for POST and PUT methods
		if (content != null && (method == HttpMethod.Post || method == HttpMethod.Put))
		{
			request.Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
		}

		HttpResponseMessage response = await _httpClient.SendAsync(request);
		var                 result   = await response.Content.ReadFromJsonAsync<RequestResult>();

		if (response.IsSuccessStatusCode)
		{
			UpdateToken(result);
			return result!;
		}
		else
		{
			return new RequestResult().SetStatus(result!.StatusCode).SetErrors(result.Errors);
		}
	}

	/// <summary>
	/// Helper method to process the HTTP response, handling success and error cases.
	/// </summary>
	/// <typeparam name="T">The type of the expected response.</typeparam>
	/// <param name="response">The HTTP response message.</param>
	/// <param name="method">The HTTP method of the request.</param>
	/// <returns>The result of the request.</returns>
	private async Task<RequestResult<T>> ProcessResponse<T>(HttpResponseMessage response, HttpMethod method)
	{
		if (response.IsSuccessStatusCode)
		{
			if (method == HttpMethod.Delete)
			{
				var result        = await response.Content.ReadFromJsonAsync<RequestResult>();
				var genericResult = new RequestResult<T>();
				if (result == null) return genericResult;
				genericResult.SetStatus(result.StatusCode).SetErrors(result.Errors);
				UpdateToken(result);

				return genericResult;
			}
			else
			{
				var result = await response.Content.ReadFromJsonAsync<RequestResult<T>>();
				UpdateToken(result);
				return result ?? new RequestResult<T>().SetErrors("Error deserializing response.");
			}
		}

		var errorResult = await response.Content.ReadFromJsonAsync<RequestResult<T>>();
		if (errorResult == null)
			return new RequestResult<T>()
			       .SetStatus((int)response.StatusCode)
			       .SetErrors($"Error: {response.ReasonPhrase}");
		UpdateToken(errorResult);
		return errorResult;
	}
}

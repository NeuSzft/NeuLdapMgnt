using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using NeuLdapMgnt.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace NeuLdapMgnt.WebApp.Requests
{
	public class ApiRequests
	{
		[Inject] public NavigationManager NavManager { get; set; } = default!;

		private HttpClient _httpClient;
		private string? _token = null;

		public event Action? AuthenticationStateChanged;

		// Property to check if a user token exists, indicating the user is authenticated
		public bool IsAuthenticated => _token != null;

		public ApiRequests(string baseUri)
		{
			// Initializes HttpClient with JSON as the default request content type
			_httpClient = new()
			{
				BaseAddress = new(baseUri),
				DefaultRequestHeaders = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } }
			};
		}

		// Redirects to login if the user is not authenticated
		public void EnsureAuthentication(NavigationManager navManager)
		{
			if (!IsAuthenticated) navManager.NavigateTo("login");
		}

		// Throws an exception if the user is not authenticated
		private void EnsureAuthentication()
		{
			if (!IsAuthenticated)
			{
				throw new InvalidOperationException("User is not authenticated.");
			}
		}

		// Performs login operation by sending credentials to the server
		public async Task<bool> LoginAsync(string username, string password)
		{
			string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

			var response = await _httpClient.GetAsync("api/auth");
			if (!response.IsSuccessStatusCode) return false;

			_token = await response.Content.ReadAsStringAsync();
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

			AuthenticationStateChanged?.Invoke();
			return true;
		}

		// Clears the authentication token and logging the user out
		public void Logout()
		{
			_token = null;
			AuthenticationStateChanged?.Invoke();
		}

		// Updates the authentication token based on the server response
		private void UpdateToken(RequestResult? result)
		{
			if (result is null || string.IsNullOrEmpty(result.NewToken))
			{
				throw new InvalidOperationException("New token is missing.");
			}

			_token = result.NewToken;
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
		}

		/// <summary>Sends a request to the specified URI with the string content in it's body, when the <paramref name="method"/> is either <c>POST</c> or <c>PUT</c>.</summary>
		public async Task<RequestResult> SendStringAsync(HttpMethod method, string uri, string? content) {
			EnsureAuthentication();

			HttpRequestMessage request = new(method, uri);

			// Sets the request content for POST and PUT methods
			if (content != null && (method == HttpMethod.Post || method == HttpMethod.Put))
				request.Content = new StringContent(content);

			HttpResponseMessage response = await _httpClient.SendAsync(request);

			// Processes the response, updating tokens as necessary and handling errors
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

			// Sets the request content for POST and PUT methods
			if (content != null && (method == HttpMethod.Post || method == HttpMethod.Put))
			{
				request.Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
			}

			HttpResponseMessage response = await _httpClient.SendAsync(request);

			// Processes the response, updating tokens as necessary and handling errors
			return await ProcessResponse<T>(response, method);
		}

		// Helper method to process the HTTP response, handling success and error cases
		private async Task<RequestResult<T>> ProcessResponse<T>(HttpResponseMessage response, HttpMethod method)
		{
			if (response.IsSuccessStatusCode)
			{
				if (method == HttpMethod.Delete)
				{
					var result = await response.Content.ReadFromJsonAsync<RequestResult>();
					var genericResult = new RequestResult<T>();
					if (result != null)
					{
						genericResult.SetStatus(result.StatusCode).SetErrors(result.Errors);
						UpdateToken(result);
					}
					return genericResult;
				}
				else
				{
					var result = await response.Content.ReadFromJsonAsync<RequestResult<T>>();
					UpdateToken(result);
					return result ?? new RequestResult<T>().SetErrors("Error deserializing response.");
				}
			}
			else
			{
				var errorResult = await response.Content.ReadFromJsonAsync<RequestResult<T>>();
				if (errorResult != null)
				{
					UpdateToken(errorResult);
					return errorResult;
				}
				else
				{
					return new RequestResult<T>()
						.SetStatus((int)response.StatusCode)
						.SetErrors($"Error: {response.ReasonPhrase}");
				}
			}
		}

		// Uploads a file to the server, specifically for importing student data via CSV
		public async Task<RequestResult> UploadStudentFileAsync(IBrowserFile file)
		{
			EnsureAuthentication();

			var stream = file.OpenReadStream();
			var content = new StreamContent(stream);
			content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

			var response = await _httpClient.PostAsync("api/students/import", content);
			response.EnsureSuccessStatusCode();

			var result = await response.Content.ReadFromJsonAsync<RequestResult>();
			UpdateToken(result);
			return result!;
		}

		public void SetBaseAddress(Uri url)
		{
			_httpClient = new()
			{
				BaseAddress = url,
				DefaultRequestHeaders = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } },
			};
			Logout();
		}

		public Uri? GetBaseAddress()
		{
			return _httpClient.BaseAddress;
		}
	}
}


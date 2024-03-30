using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using NeuLdapMgnt.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace NeuLdapMgnt.WebApp.Client
{
	public class ApiRequests
	{
		private readonly HttpClient _httpClient;
		private readonly Uri _url = new("http://localhost:5000");
		private string? _token = null;

		public event Action? AuthenticationStateChanged;
		public bool IsAuthenticated => _token != null;

		public ApiRequests()
		{
			_httpClient = new()
			{
				BaseAddress = _url,
				DefaultRequestHeaders = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } }
			};
		}

		public void EnsureAuthentication(NavigationManager navManager)
		{
			if (!IsAuthenticated) navManager.NavigateTo("login");
		}

		private void EnsureAuthentication()
		{
			if (!IsAuthenticated)
			{
				throw new InvalidOperationException("User is not authenticated.");
			}
		}

		public async Task LoginAsync(string username, string password)
		{
			string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

			var response = await _httpClient.GetAsync("/auth");
			response.EnsureSuccessStatusCode();

			_token = await response.Content.ReadAsStringAsync();
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

			AuthenticationStateChanged?.Invoke();
		}

		public void Logout()
		{
			_token = null;
			AuthenticationStateChanged?.Invoke();
		}

		private void UpdateToken(RequestResult? result)
		{
			if (result is null || string.IsNullOrEmpty(result.NewToken))
			{
				throw new InvalidOperationException("New token is missing.");
			}

			_token = result.NewToken;
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
		}

		public async Task<RequestResult<T>> SendRequestAsync<T>(HttpMethod method, string uri, object? content = null)
		{
			EnsureAuthentication();

			HttpRequestMessage request = new HttpRequestMessage(method, uri);

			if (content != null && (method == HttpMethod.Post || method == HttpMethod.Put))
			{
				request.Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
			}

			HttpResponseMessage response = await _httpClient.SendAsync(request);

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

		public async Task<RequestResult<Student>?> GetStudentsAsync()
		{
			var result = await SendRequestAsync<Student>(HttpMethod.Get, "/students");
			return result ?? null;
		}

		public async Task<RequestResult<Student>?> AddStudentAsync(Student student)
		{
			var result = await SendRequestAsync<Student>(HttpMethod.Post, "/students", student);
			return result ?? null;
		}

		public async Task<RequestResult<Student>?> UpdateStudentAsync(long id, Student student)
		{
			var result = await SendRequestAsync<Student>(HttpMethod.Put, $"/students/{id}", student);
			return result ?? null;
		}

		public async Task<RequestResult<Student>?> DeleteStudentAsync(long id)
		{
			var result = await SendRequestAsync<Student>(HttpMethod.Delete, $"/students/{id}");
			return result ?? null;
		}

		public async Task<RequestResult?> UploadFileAsync(IBrowserFile file)
		{
			EnsureAuthentication();

			var stream = file.OpenReadStream();
			var content = new StreamContent(stream);
			content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

			var response = await _httpClient.PostAsync("/students/import", content);
			response.EnsureSuccessStatusCode();

			var result = await response.Content.ReadFromJsonAsync<RequestResult>();
			UpdateToken(result);

			return result ?? null;
		}
	}
}


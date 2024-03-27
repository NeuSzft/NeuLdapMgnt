using Microsoft.AspNetCore.Components;
using NeuLdapMgnt.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace NeuLdapMgnt.WebApp.Client
{
	public class ApiRequests
	{
		private readonly HttpClient _httpClient;
		private readonly Uri _url = new("http://localhost:5000");
		private string? _token = null;

		public event Action? AuthenticationStateChanged;
		public bool IsAuthenticated => true /*|| _token != null*/;

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
			var response = await _httpClient.PostAsJsonAsync("/login", new { username, password });
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

		public async Task GetToken()
		{
			var response = await _httpClient.GetAsync("/auth/token");
			response.EnsureSuccessStatusCode();

			_token = await response.Content.ReadAsStringAsync();
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

			AuthenticationStateChanged?.Invoke();
		}

		public async Task GetTest()
		{
			var response = await _httpClient.GetAsync("/auth/test");
			response.EnsureSuccessStatusCode();
		}

		public async Task<IEnumerable<Student>> GetStudentsAsync()
		{
			EnsureAuthentication();
			var response = await _httpClient.GetFromJsonAsync<IEnumerable<Student>>("/students");
			return response ?? Enumerable.Empty<Student>();
		}

		public async Task<Student> AddStudentAsync(Student student)
		{
			EnsureAuthentication();

			var response = await _httpClient.PostAsJsonAsync("/students", student);
			response.EnsureSuccessStatusCode();

			var content = await response.Content.ReadFromJsonAsync<Student>();
			return content!;
		}

		public async Task UpdateStudentAsync(Student student)
		{
			EnsureAuthentication();
			var response = await _httpClient.PutAsJsonAsync($"/students/{student.Id}", student);
			response.EnsureSuccessStatusCode();
		}

		public async Task DeleteStudentAsync(int id)
		{
			EnsureAuthentication();
			var response = await _httpClient.DeleteAsync($"/students/{id}");
			response.EnsureSuccessStatusCode();
		}
	}
}


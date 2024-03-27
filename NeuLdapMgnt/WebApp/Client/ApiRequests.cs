using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using NeuLdapMgnt.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

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

		private void UpdateToken(HttpResponseMessage response)
		{
			var authHeader = response.Headers.FirstOrDefault(x => string.Equals(x.Key, "New-Authorization", StringComparison.OrdinalIgnoreCase));

			if (authHeader.Value is null)
			{
				throw new ArgumentException("Something went wrong");
			}

			_token = authHeader.Value.FirstOrDefault()!.Replace("Bearer ", "");
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
		}

		public async Task<IEnumerable<Student>> GetStudentsAsync()
		{
			EnsureAuthentication();

			var response = await _httpClient.GetAsync("/students");
			response.EnsureSuccessStatusCode();
			UpdateToken(response);

			var students = await response.Content.ReadFromJsonAsync<IEnumerable<Student>>();

			return students ?? Enumerable.Empty<Student>();
		}

		public async Task<Student> AddStudentAsync(Student student)
		{
			EnsureAuthentication();

			var response = await _httpClient.PostAsJsonAsync("/students", student);
			response.EnsureSuccessStatusCode();
			UpdateToken(response);

			var content = await response.Content.ReadFromJsonAsync<Student>();
			return content!;
		}

		public async Task UpdateStudentAsync(Student student)
		{
			EnsureAuthentication();

			var response = await _httpClient.PutAsJsonAsync($"/students/{student.Id}", student);
			response.EnsureSuccessStatusCode();
			UpdateToken(response);
		}

		public async Task DeleteStudentAsync(int id)
		{
			EnsureAuthentication();

			var response = await _httpClient.DeleteAsync($"/students/{id}");
			response.EnsureSuccessStatusCode();
			UpdateToken(response);
		}

		public async Task UploadFile(IBrowserFile file)
		{
			EnsureAuthentication();

			var stream = file.OpenReadStream();

			var content = new StreamContent(stream);
			content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

			var response = await _httpClient.PostAsync("/management/refill", content);
			response.EnsureSuccessStatusCode();
			UpdateToken(response);
		}
	}
}


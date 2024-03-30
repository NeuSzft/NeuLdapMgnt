using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.WebApp.Client.Requests
{
	public static class StudentRequests
	{
		// Sends a GET request to retrieve all student entries
		public static async Task<RequestResult<Student>?> GetStudentsAsync(this ApiRequests apiRequests)
		{
			var result = await apiRequests.SendRequestAsync<Student>(HttpMethod.Get, "/students");
			return result ?? null;
		}

		// Sends a POST request to create a new student entry
		public static async Task<RequestResult<Student>?> AddStudentAsync(this ApiRequests apiRequests, Student student)
		{
			var result = await apiRequests.SendRequestAsync<Student>(HttpMethod.Post, "/students", student);
			return result ?? null;
		}

		// Sends a PUT request to update the student by their ID
		public static async Task<RequestResult<Student>?> UpdateStudentAsync(this ApiRequests apiRequests, long id, Student student)
		{
			var result = await apiRequests.SendRequestAsync<Student>(HttpMethod.Put, $"/students/{id}", student);
			return result ?? null;
		}

		// Sends a DELETE request to delete the student by their ID
		public static async Task<RequestResult<Student>?> DeleteStudentAsync(this ApiRequests apiRequests, long id)
		{
			var result = await apiRequests.SendRequestAsync<Student>(HttpMethod.Delete, $"/students/{id}");
			return result ?? null;
		}
	}
}

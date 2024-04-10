using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.WebApp.Requests
{
	public static class StudentRequests
	{
		// Sends a GET request to retrieve all student entries
		public static async Task<RequestResult<Student>?> GetStudentsAsync(this ApiRequests apiRequests)
			=> await apiRequests.SendRequestAsync<Student>(HttpMethod.Get, "api/students");

		// Sends a POST request to create a new student entry
		public static async Task<RequestResult<Student>?> AddStudentAsync(this ApiRequests apiRequests, Student student)
			=> await apiRequests.SendRequestAsync<Student>(HttpMethod.Post, "api/students", student);

		// Sends a PUT request to update the student by their ID
		public static async Task<RequestResult<Student>?> UpdateStudentAsync(this ApiRequests apiRequests, long id, Student student)
			=> await apiRequests.SendRequestAsync<Student>(HttpMethod.Put, $"api/students/{id}", student);

		// Sends a DELETE request to delete the student by their ID
		public static async Task<RequestResult<Student>?> DeleteStudentAsync(this ApiRequests apiRequests, long id)
			=> await apiRequests.SendRequestAsync<Student>(HttpMethod.Delete, $"api/students/{id}");
	}
}

using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.WebApp.Requests
{
	public static class StudentRequests
	{
		public static async Task<RequestResult<Student>> GetStudentsAsync(this ApiRequests apiRequests)
			=> await apiRequests.SendRequestAsync<Student>(HttpMethod.Get, "api/students");

		public static async Task<RequestResult<Student>> AddStudentAsync(this ApiRequests apiRequests, Student student)
			=> await apiRequests.SendRequestAsync<Student>(HttpMethod.Post, "api/students", student);

		public static async Task<RequestResult<Student>> UpdateStudentAsync(this ApiRequests apiRequests, long id, Student student)
			=> await apiRequests.SendRequestAsync<Student>(HttpMethod.Put, $"api/students/{id}", student);

		public static async Task<RequestResult<Student>> DeleteStudentAsync(this ApiRequests apiRequests, long id)
			=> await apiRequests.SendRequestAsync<Student>(HttpMethod.Delete, $"api/students/{id}");
	}
}

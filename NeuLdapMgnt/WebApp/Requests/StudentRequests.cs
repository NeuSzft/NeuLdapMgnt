using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.WebApp.Requests;

public static class StudentRequests {
	public static async Task<RequestResult<Student>> GetStudentsAsync(this ApiRequests apiRequests)
		=> await apiRequests.SendRequestAsync<Student>(HttpMethod.Get, "api/students");

	public static async Task<RequestResult<Student>> GetStudentAsync(this ApiRequests apiRequests, string id)
		=> await apiRequests.SendRequestAsync<Student>(HttpMethod.Get, $"api/students/{id}");

	public static async Task<RequestResult<Student>> AddStudentAsync(this ApiRequests apiRequests, Student student, bool setPassword)
		=> await apiRequests.SendRequestAsync<Student>(HttpMethod.Post, $"api/students?pwd={setPassword}", student);

	public static async Task<RequestResult<Student>> UpdateStudentAsync(this ApiRequests apiRequests, string id, Student student, bool setPassword)
		=> await apiRequests.SendRequestAsync<Student>(HttpMethod.Put, $"api/students/{id}?pwd={setPassword}", student);

	public static async Task<RequestResult> DeleteStudentAsync(this ApiRequests apiRequests, string id)
		=> await apiRequests.SendRequestAsync(HttpMethod.Delete, $"api/students/{id}");
}

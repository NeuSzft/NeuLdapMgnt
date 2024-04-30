using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.WebApp.Requests;

public static class TeacherRequests
{
	public static async Task<RequestResult<Teacher>> GetTeachersAsync(this ApiRequests apiRequests)
		=> await apiRequests.SendRequestAsync<Teacher>(HttpMethod.Get, "api/teachers");

	public static async Task<RequestResult<Teacher>> GetTeacherAsync(this ApiRequests apiRequests, string id)
		=> await apiRequests.SendRequestAsync<Teacher>(HttpMethod.Get, $"api/teachers/{id}");

	public static async Task<RequestResult<Teacher>> AddTeacherAsync(this ApiRequests apiRequests, Teacher teacher, bool setPassword)
		=> await apiRequests.SendRequestAsync<Teacher>(HttpMethod.Post, $"api/teachers?pwd={setPassword}", teacher);

	public static async Task<RequestResult<Teacher>> UpdateTeacherAsync(this ApiRequests apiRequests, string id, Teacher teacher, bool setPassword)
		=> await apiRequests.SendRequestAsync<Teacher>(HttpMethod.Put, $"api/teachers/{id}?pwd={setPassword}", teacher);

	public static async Task<RequestResult> DeleteTeacherAsync(this ApiRequests apiRequests, string id)
		=> await apiRequests.SendRequestAsync(HttpMethod.Delete, $"api/teachers/{id}");
}

using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.WebApp.Requests
{
	public static class AdminRequests
	{
		public static async Task<RequestResult<string>> GetAdminsAsync(this ApiRequests apiRequests)
			=> await apiRequests.SendRequestAsync<string>(HttpMethod.Get, "api/admins");

		public static async Task<RequestResult> AddAdminAsync(this ApiRequests apiRequests, string id)
			=> await apiRequests.SendRequestAsync(HttpMethod.Post, $"api/admins/{id}");

		public static async Task<RequestResult> DeleteAdminAsync(this ApiRequests apiRequests, string id)
			=> await apiRequests.SendRequestAsync(HttpMethod.Delete, $"api/admins/{id}");
	}
}

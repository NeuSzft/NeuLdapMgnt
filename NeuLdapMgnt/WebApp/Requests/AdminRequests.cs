using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.WebApp.Requests
{
	public static class AdminRequests
	{
		public static async Task<RequestResult<string>> GetAdminsAsync(this ApiRequests apiRequests)
			=> await apiRequests.SendRequestAsync<string>(HttpMethod.Get, "api/admins");
	}
}

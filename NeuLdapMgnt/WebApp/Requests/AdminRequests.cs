using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.WebApp.Requests
{
	public static class AdminRequests
	{
		public static async Task<RequestResult<IEnumerable<string>>> GetAdminsAsync(this ApiRequests apiRequests)
			=> await apiRequests.SendRequestAsync<IEnumerable<string>>(HttpMethod.Get, "api/admins");
	}
}

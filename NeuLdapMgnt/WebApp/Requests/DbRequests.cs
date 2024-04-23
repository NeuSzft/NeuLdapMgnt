using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.WebApp.Requests
{
	public static class DbRequests
	{
		public static async Task<RequestResult<LogEntry>> GetLogsAsync(this ApiRequests apiRequests, long from, long to)
			=> await apiRequests.SendRequestAsync<LogEntry>(HttpMethod.Get, $"/api/logs?from={from}&to={to}");

		public static async Task<RequestResult<string[]>> GetClassesAsync(this ApiRequests apiRequests)
			=> await apiRequests.SendRequestAsync<string[]>(HttpMethod.Get, $"/api/classes");

		public static async Task<RequestResult<IEnumerable<string>>> AddClassesAsync(this ApiRequests apiRequests, IEnumerable<string> classes)
			=> await apiRequests.SendRequestAsync<IEnumerable<string>>(HttpMethod.Put, $"/api/classes", classes);
	}
}

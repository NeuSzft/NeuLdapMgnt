using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.WebApp.Requests;

public static class EmployeeRequests
{
	public static async Task<RequestResult<Employee>> GetEmployeesAsync(this ApiRequests apiRequests)
		=> await apiRequests.SendRequestAsync<Employee>(HttpMethod.Get, "api/employees");

	public static async Task<RequestResult<Employee>> GetEmployeeAsync(this ApiRequests apiRequests, string id)
		=> await apiRequests.SendRequestAsync<Employee>(HttpMethod.Get, $"api/employees/{id}");

	public static async Task<RequestResult<Employee>> AddEmployeeAsync(this ApiRequests apiRequests, Employee employee, bool setPassword)
		=> await apiRequests.SendRequestAsync<Employee>(HttpMethod.Post, $"api/employees?pwd={setPassword}", employee);

	public static async Task<RequestResult<Employee>> UpdateEmployeeAsync(this ApiRequests apiRequests, string id, Employee employee, bool setPassword)
		=> await apiRequests.SendRequestAsync<Employee>(HttpMethod.Put, $"api/employees/{id}?pwd={setPassword}", employee);

	public static async Task<RequestResult> DeleteEmployeeAsync(this ApiRequests apiRequests, string id)
		=> await apiRequests.SendRequestAsync(HttpMethod.Delete, $"api/employees/{id}");
}

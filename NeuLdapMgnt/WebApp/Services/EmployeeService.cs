using Microsoft.AspNetCore.Components;
using NeuLdapMgnt.Models;
using NeuLdapMgnt.WebApp.Requests;

namespace NeuLdapMgnt.WebApp.Services;

/// <summary>
/// Service for managing employees.
/// </summary>
public class EmployeeService
{
	[Inject] private ApiRequests ApiRequests { get; set; }

	[Inject] private LocalDbService DatabaseService { get; set; }

	[Inject] private NotificationService NotificationService { get; set; }

	public List<Employee> Employees { get; set; } = new();

	/// <summary>
	/// Constructor for EmployeeService.
	/// </summary>
	/// <param name="apiRequests">Instance of ApiRequests for API communication.</param>
	/// <param name="localDbService">Instance of LocalDbService for local database operations.</param>
	/// <param name="notificationService">Instance of NotificationService for handling notifications.</param>
	public EmployeeService(ApiRequests apiRequests, LocalDbService localDbService, NotificationService notificationService)
	{
		ApiRequests = apiRequests;
		DatabaseService = localDbService;
		NotificationService = notificationService;
	}

	/// <summary>
	/// Fetches employees asynchronously.
	/// </summary>
	public async Task FetchEmployeesAsync()
	{
		try
		{
			var response = await ApiRequests.GetEmployeesAsync();
			if (response.IsSuccess())
			{
				Employees.Clear();
				foreach (var employee in response.Values)
				{
					employee.IsInactive = DatabaseService.InactiveUsers.Contains(employee.Id);
					employee.IsAdmin = DatabaseService.Admins.Contains(employee.Id);
					Employees.Add(employee);
				}
			}
			else
			{
				NotificationService.NotifyError(response.GetError());
			}
		}
		catch (Exception e)
		{
			NotificationService.HandleError(e);
		}
	}

	/// <summary>
	/// Fetches a employee asynchronously by ID.
	/// </summary>
	/// <param name="id">The ID of the employee to fetch.</param>
	/// <returns>The fetched employee, or null if not found.</returns>
	public async Task<Employee?> FetchEmployeeAsync(string id)
	{
		try
		{
			var response = await ApiRequests.GetEmployeeAsync(id);
			if (response.IsSuccess())
			{
				Employee employee = response.Values[0];
				employee.IsInactive = DatabaseService.InactiveUsers.Contains(employee.Id);
				employee.IsAdmin = DatabaseService.Admins.Contains(employee.Id);
				return employee;
			}

			NotificationService.NotifyError(response.GetError());
		}
		catch (Exception e)
		{
			NotificationService.HandleError(e);
		}

		return null;
	}

	/// <summary>
	/// Updates a employee asynchronously.
	/// </summary>
	/// <param name="employee">The employee to update.</param>
	/// <returns>A list of error messages encountered during the update.</returns>
	public async Task<List<string>> UpdateEmployeeAsync(Employee employee)
	{
		List<string> errorList = new();
		try
		{
			var response = await ApiRequests.UpdateEmployeeAsync(employee.Id, employee, !string.IsNullOrWhiteSpace(employee.Password));

			if (response.IsSuccess())
			{
				NotificationService.NotifySuccess($"{employee.FullName} was updated!");
			}
			else
			{
				errorList.AddRange(response.Errors);
			}
		}
		catch (Exception e)
		{
			NotificationService.HandleError(e);
		}

		return errorList;
	}

	/// <summary>
	/// Updates multiple employees asynchronously.
	/// </summary>
	/// <param name="employees">The list of employees to update.</param>
	/// <param name="isAdmin">Flag indicating whether the employees should be set as administrators.</param>
	/// <param name="isInactive">Flag indicating whether the employees should be set as inactive.</param>
	/// <returns>A list of error messages encountered during the update.</returns>
	public async Task<List<string>> UpdateEmployeesAsync(List<Employee> employees, bool isAdmin, bool isInactive)
	{
		List<string> errorList = new();
		try
		{
			if (isInactive)
			{
				var responseInactive = await ApiRequests.GetInactiveUsersAsync();
				DatabaseService.InactiveUsers = new(responseInactive.Values);
			}

			var responseStatus = await UpdateEmployeesStatusAsync(employees, isAdmin, isInactive, errorList);
			errorList = responseStatus.ToList();

			if (isInactive) await FetchEmployeesAsync();
			if (isAdmin) await DatabaseService.FetchAdminsAsync();

			NotificationService.NotifySuccess($"Status was updated for {employees.Count} employee(s)");
		}
		catch (Exception e)
		{
			NotificationService.HandleError(e);
		}

		return errorList;
	}

	/// <summary>
	/// Updates the status of multiple employees asynchronously.
	/// </summary>
	/// <param name="employees">The list of employees to update.</param>
	/// <param name="isAdmin">Flag indicating whether the employees should be set as administrators.</param>
	/// <param name="isInactive">Flag indicating whether the employees should be set as inactive.</param>
	/// <param name="errorList">The list of errors encountered during the update.</param>
	/// <returns>A list of error messages encountered during the update.</returns>
	private async Task<List<string>> UpdateEmployeesStatusAsync(List<Employee> employees, bool isAdmin, bool isInactive, List<string> errorList)
	{
		foreach (var employee in employees)
		{
			if (isAdmin && !DatabaseService.Admins.Contains(employee.Id))
			{
				var responseAdd = await ApiRequests.AddAdminAsync(employee.Id);
				if (responseAdd.IsFailure())
				{
					errorList.AddRange(responseAdd.Errors);
				}
			}

			if (isInactive && !DatabaseService.InactiveUsers.Contains(employee.Id))
			{
				var responseDeactivate = await ApiRequests.DeactivateUserAsync(employee.Id);
				if (responseDeactivate.IsFailure())
				{
					errorList.AddRange(responseDeactivate.Errors);
				}
			}

			var responseUpdate = await ApiRequests.UpdateEmployeeAsync(employee.Id, employee, !string.IsNullOrWhiteSpace(employee.Password));
			if (responseUpdate.IsFailure())
			{
				errorList.AddRange(responseUpdate.Errors);
			}
		}

		return errorList;
	}

	/// <summary>
	/// Adds a new employee asynchronously.
	/// </summary>
	/// <param name="employee">The employee to add.</param>
	public async Task AddEmployeeAsync(Employee employee)
	{
		try
		{
			var response = await ApiRequests.AddEmployeeAsync(employee, !string.IsNullOrWhiteSpace(employee.Password));
			if (response.IsSuccess())
			{
				NotificationService.NotifySuccess($"{employee.FullName} was added!");
			}
			else
			{
				NotificationService.NotifyError(response.GetError());
			}
		}
		catch (Exception e)
		{
			NotificationService.HandleError(e);
		}
	}
}

using Microsoft.AspNetCore.Components;
using NeuLdapMgnt.Models;
using NeuLdapMgnt.WebApp.Requests;

namespace NeuLdapMgnt.WebApp.Services;

/// <summary>
/// Service for interacting with the local database.
/// </summary>
public class LocalDbService
{
	[Inject] private ApiRequests ApiRequests { get; set; }

	[Inject] private NotificationService NotificationService { get; set; }

	public List<string> Admins { get; set; } = new();

	public List<string> Classes { get; set; } = new();

	public List<string> InactiveUsers { get; set; } = new();

	/// <summary>
	/// Constructor for LocalDbService.
	/// </summary>
	/// <param name="apiRequests">Instance of ApiRequests for API communication.</param>
	/// <param name="notificationService">Instance of NotificationService for handling notifications.</param>
	public LocalDbService(ApiRequests apiRequests, NotificationService notificationService)
	{
		ApiRequests = apiRequests;
		NotificationService = notificationService;
	}

	/// <summary>
	/// Fetches administrators asynchronously.
	/// </summary>
	public async Task FetchAdminsAsync()
	{
		try
		{
			var response = await ApiRequests.GetAdminsAsync();
			if (response.IsSuccess())
			{
				Admins = new(response.Values);
			}

			if (response.Errors.Any())
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
	/// Fetches inactive users asynchronously.
	/// </summary>
	public async Task FetchInactiveUsersAsync()
	{
		try
		{
			var response = await ApiRequests.GetInactiveUsersAsync();
			if (response.IsSuccess())
			{
				InactiveUsers = new(response.Values);
			}

			if (response.Errors.Any())
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
	/// Fetches classes asynchronously.
	/// </summary>
	public async Task FetchClassesAsync()
	{
		try
		{
			var response = await ApiRequests.GetClassesAsync();
			if (response.IsSuccess())
			{
				Classes = response.Values.OrderBy(Utils.GetClassOrderValue).ToList();
			}

			if (response.Errors.Any())
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
	/// Deactivates a user asynchronously.
	/// </summary>
	/// <param name="user">The user to deactivate.</param>
	public async Task DeactivateUserAsync(Person user)
	{
		try
		{
			user.IsInactive = true;
			if (user is Student student)
			{
				var response = await StudentRequests.UpdateStudentAsync(ApiRequests, student.Id.ToString(), student, false);
				HandleNotification(response, $"{student.FullName}'s status was set to [Inactive]");
			}
			else if (user is Employee employee)
			{
				var response = await EmployeeRequests.UpdateEmployeeAsync(ApiRequests, employee.Id, employee, false);
				HandleNotification(response, $"{employee.FullName}'s status was set to [Inactive]");
			}
		}
		catch (Exception e)
		{
			NotificationService.HandleError(e);
		}
	}

	private void HandleNotification<T>(RequestResult<T> result, string message)
	{
		if (result.IsSuccess())
		{
			NotificationService.NotifySuccess(message);
		}

		if (result.Errors.Any())
		{
			NotificationService.NotifyError(result.GetError());
		}
	}

	/// <summary>
	/// Activates users asynchronously.
	/// </summary>
	/// <param name="users">List of users to activate.</param>
	/// <returns>A list of error messages encountered during activation.</returns>
	public async Task<List<string>> ActivateUsersAsync(List<Person> users)
	{
		List<string> errorList = new();
		try
		{
			foreach (var user in users)
			{
				user.IsInactive = false;

				if (user is Student student)
				{
					var response = await StudentRequests.UpdateStudentAsync(ApiRequests, student.Id.ToString(), student, false);
					if (response.IsFailure())
					{
						errorList.AddRange(response.Errors);
					}
				}
				else if (user is Employee employee)
				{
					var response = await EmployeeRequests.UpdateEmployeeAsync(ApiRequests, employee.Id.ToString(), employee, false);
					if (response.IsFailure())
					{
						errorList.AddRange(response.Errors);
					}
				}
			}
		}
		catch (Exception e)
		{
			NotificationService.HandleError(e);
		}

		return errorList;
	}

	/// <summary>
	/// Deletes users asynchronously.
	/// </summary>
	/// <param name="users">List of users to delete.</param>
	/// <returns>A list of error messages encountered during deletion.</returns>
	public async Task<List<string>> DeleteUsersAsync(List<Person> users)
	{
		List<string> errorList = new();
		try
		{
			foreach (var user in users)
			{
				if (user is Student student)
				{
					var response = await StudentRequests.DeleteStudentAsync(ApiRequests, student.Id.ToString());
					if (response.IsFailure())
					{
						errorList.AddRange(response.Errors);
					}
				}
				else if (user is Employee employee)
				{
					var response = await EmployeeRequests.DeleteEmployeeAsync(ApiRequests, employee.Id.ToString());
					if (response.IsFailure())
					{
						errorList.AddRange(response.Errors);
					}
				}
			}
		}
		catch (Exception e)
		{
			NotificationService.HandleError(e);
		}

		return errorList;
	}

	/// <summary>
	/// Deletes administrators asynchronously.
	/// </summary>
	/// <param name="admins">List of administrators to delete.</param>
	/// <returns>A list of error messages encountered during deletion.</returns>
	public async Task<List<string>> DeleteAdminsAsync(List<Employee> employees)
	{
		List<string> errorList = new();
		try
		{
			foreach (var employee in employees)
			{
				employee.IsAdmin = false;
				var response = await EmployeeRequests.UpdateEmployeeAsync(ApiRequests, employee.Id, employee, false);
				if (response.IsFailure())
				{
					errorList.Add(response.GetError());
				}
			}
		}
		catch (Exception e)
		{
			NotificationService.HandleError(e);
		}

		return errorList;
	}
}

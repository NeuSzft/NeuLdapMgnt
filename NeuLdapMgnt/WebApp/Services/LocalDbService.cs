using Microsoft.AspNetCore.Components;
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
				Classes = response.Values.SelectMany(x => x).OrderBy(Utils.GetClassOrderValue).ToList();
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
	/// <param name="id">The ID of the user to deactivate.</param>
	public async Task DeactivateUserAsync(string id)
	{
		try
		{
			var response = await ApiRequests.DeactivateUserAsync(id);
			if (response.IsSuccess())
			{
				InactiveUsers.Remove(id);
				NotificationService.NotifySuccess($"{id} status was set to [Inactive]");
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
	/// Activates users asynchronously.
	/// </summary>
	/// <param name="ids">The IDs of users to activate.</param>
	/// <returns>A list of error messages encountered during activation.</returns>
	public async Task<List<string>> ActivateUsersAsync(List<string> ids)
	{
		List<string> errorList = new();
		try
		{
			await FetchInactiveUsersAsync();
			foreach (string id in ids)
			{
				var response = await ApiRequests.ActivateUserAsync(id);
				if (response.IsSuccess())
				{
					InactiveUsers.Remove(id);
				}
				else
				{
					errorList.AddRange(response.Errors);
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
	/// <param name="ids">The IDs of users to delete.</param>
	/// <returns>A list of error messages encountered during deletion.</returns>
	public async Task<List<string>> DeleteUsersAsync(List<string> ids)
	{
		List<string> errorList = new();
		try
		{
			foreach (var id in ids)
			{
				if (Utils.IsStudent(id))
				{
					var response = await ApiRequests.DeleteStudentAsync(id);
					if (response.IsFailure())
					{
						errorList.Add(response.GetError());
					}
				}
				else
				{
					var response = await ApiRequests.DeleteEmployeeAsync(id);
					if (response.IsFailure())
					{
						errorList.Add(response.GetError());
					}
					else if (response.IsSuccess() && Admins.Contains(id))
					{
						await ApiRequests.DeleteAdminAsync(id);
					}
				}
			}

			await ActivateUsersAsync(ids.Where(x => !errorList.Contains(x)).ToList());
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
	/// <param name="ids">The IDs of administrators to delete.</param>
	/// <returns>A list of error messages encountered during deletion.</returns>
	public async Task<List<string>> DeleteAdminsAsync(List<string> ids)
	{
		List<string> errorList = new();
		try
		{
			foreach (var id in ids)
			{
				var response = await ApiRequests.DeleteAdminAsync(id);
				if (response.IsFailure())
				{
					errorList.Add(response.GetError());
				}
				else
				{
					Admins.Remove(id);
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

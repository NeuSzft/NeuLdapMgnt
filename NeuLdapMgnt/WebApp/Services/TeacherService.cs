using Microsoft.AspNetCore.Components;
using NeuLdapMgnt.Models;
using NeuLdapMgnt.WebApp.Requests;

namespace NeuLdapMgnt.WebApp.Services;

/// <summary>
/// Service for managing teachers.
/// </summary>
public class TeacherService
{
	[Inject] private ApiRequests ApiRequests { get; set; }

	[Inject] private LocalDbService DatabaseService { get; set; }

	[Inject] private NotificationService NotificationService { get; set; }

	public List<Teacher> Teachers { get; set; } = new();

	/// <summary>
	/// Constructor for TeacherService.
	/// </summary>
	/// <param name="apiRequests">Instance of ApiRequests for API communication.</param>
	/// <param name="localDbService">Instance of LocalDbService for local database operations.</param>
	/// <param name="notificationService">Instance of NotificationService for handling notifications.</param>
	public TeacherService(ApiRequests apiRequests, LocalDbService localDbService, NotificationService notificationService)
	{
		ApiRequests = apiRequests;
		DatabaseService = localDbService;
		NotificationService = notificationService;
	}

	/// <summary>
	/// Fetches teachers asynchronously.
	/// </summary>
	public async Task FetchTeachersAsync()
	{
		try
		{
			Teachers.Clear();

			var response = await ApiRequests.GetTeachersAsync();
			if (response.IsSuccess())
			{
				Teachers = new(response.Values.Where(x =>
					!DatabaseService.InactiveUsers.Contains(x.Id.ToString())));
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
	/// Fetches a teacher asynchronously by ID.
	/// </summary>
	/// <param name="id">The ID of the teacher to fetch.</param>
	/// <returns>The fetched teacher, or null if not found.</returns>
	public async Task<Teacher?> FetchTeacherAsync(string id)
	{
		try
		{
			var response = await ApiRequests.GetTeacherAsync(id);
			if (response.IsSuccess())
			{
				return response.Values[0];
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
	/// Updates a teacher asynchronously.
	/// </summary>
	/// <param name="teacher">The teacher to update.</param>
	/// <returns>A list of error messages encountered during the update.</returns>
	public async Task<List<string>> UpdateTeacherAsync(Teacher teacher)
	{
		List<string> errorList = new();
		try
		{
			var response = await ApiRequests.UpdateTeacherAsync(teacher.Id, teacher, !string.IsNullOrWhiteSpace(teacher.Password));

			if (response.IsSuccess())
			{
				NotificationService.NotifySuccess($"{teacher.FullName} was updated!");
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
	/// Updates multiple teachers asynchronously.
	/// </summary>
	/// <param name="teachers">The list of teachers to update.</param>
	/// <param name="isAdmin">Flag indicating whether the teachers should be set as administrators.</param>
	/// <param name="isInactive">Flag indicating whether the teachers should be set as inactive.</param>
	/// <returns>A list of error messages encountered during the update.</returns>
	public async Task<List<string>> UpdateTeachersAsync(List<Teacher> teachers, bool isAdmin, bool isInactive)
	{
		List<string> errorList = new();
		try
		{
			if (isInactive)
			{
				var responseInactive = await ApiRequests.GetInactiveUsersAsync();
				DatabaseService.InactiveUsers = new(responseInactive.Values[0]);
			}

			var responseStatus = await UpdateTeachersStatusAsync(teachers, isAdmin, isInactive, errorList);
			errorList = responseStatus.ToList();

			if (isInactive) await FetchTeachersAsync();
			if (isAdmin) await DatabaseService.FetchAdminsAsync();

			NotificationService.NotifySuccess($"Status was updated for {teachers.Count} teacher(s)");
		}
		catch (Exception e)
		{
			NotificationService.HandleError(e);
		}

		return errorList;
	}

	/// <summary>
	/// Updates the status of multiple teachers asynchronously.
	/// </summary>
	/// <param name="teachers">The list of teachers to update.</param>
	/// <param name="isAdmin">Flag indicating whether the teachers should be set as administrators.</param>
	/// <param name="isInactive">Flag indicating whether the teachers should be set as inactive.</param>
	/// <param name="errorList">The list of errors encountered during the update.</param>
	/// <returns>A list of error messages encountered during the update.</returns>
	private async Task<List<string>> UpdateTeachersStatusAsync(List<Teacher> teachers, bool isAdmin, bool isInactive, List<string> errorList)
	{
		foreach (var teacher in teachers)
		{
			if (isAdmin && !DatabaseService.Admins.Contains(teacher.Id))
			{
				var responseAdd = await ApiRequests.AddAdminAsync(teacher.Id);
				if (responseAdd.IsFailure())
				{
					errorList.AddRange(responseAdd.Errors);
				}
			}

			if (isInactive && !DatabaseService.InactiveUsers.Contains(teacher.Id))
			{
				var responseDeactivate = await ApiRequests.DeactivateUserAsync(teacher.Id);
				if (responseDeactivate.IsFailure())
				{
					errorList.AddRange(responseDeactivate.Errors);
				}
			}

			var responseUpdate = await ApiRequests.UpdateTeacherAsync(teacher.Id, teacher, !string.IsNullOrWhiteSpace(teacher.Password));
			if (responseUpdate.IsFailure())
			{
				errorList.AddRange(responseUpdate.Errors);
			}
		}

		return errorList;
	}

	/// <summary>
	/// Adds a new teacher asynchronously.
	/// </summary>
	/// <param name="teacher">The teacher to add.</param>
	public async Task AddTeacherAsync(Teacher teacher)
	{
		try
		{
			var response = await ApiRequests.AddTeacherAsync(teacher, !string.IsNullOrWhiteSpace(teacher.Password));
			if (response.IsSuccess())
			{
				NotificationService.NotifySuccess($"{teacher.FullName} was added!");
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

using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using NeuLdapMgnt.Models;
using NeuLdapMgnt.WebApp.Requests;

namespace NeuLdapMgnt.WebApp.Services;

public class TeacherService
{
	[Inject] private ApiRequests ApiRequests { get; set; }

	[Inject] private LocalDbService DatabaseService { get; set; }

	[Inject] private NotificationService NotificationService { get; set; }

	public List<Teacher> Teachers { get; set; } = new();

	public TeacherService(ApiRequests apiRequests, LocalDbService localDbService,
		NotificationService notificationService)
	{
		ApiRequests = apiRequests;
		DatabaseService = localDbService;
		NotificationService = notificationService;
	}

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

	public async Task<List<string>> UpdateTeacherAsync(Teacher teacher)
	{
		List<string> errorList = new();
		try
		{
			var response = await ApiRequests.UpdateTeacherAsync(teacher.Id, teacher);

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

	public async Task<List<string>> UpdateTeachersStatusAsync(List<Teacher> teachers, bool isAdmin,
		bool isInactive, List<string> errorList)
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

			var responseUpdate = await ApiRequests.UpdateTeacherAsync(teacher.Id, teacher);
			if (responseUpdate.IsFailure())
			{
				errorList.AddRange(responseUpdate.Errors);
			}
		}

		return errorList;
	}

	public async Task AddTeacherAsync(Teacher teacher)
	{
		try
		{
			var response = await ApiRequests.AddTeacherAsync(teacher);
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

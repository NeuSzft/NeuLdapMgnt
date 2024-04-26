﻿using BlazorBootstrap;
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

	public async Task FetchTeachers()
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

	public async Task<Teacher?> FetchTeacher(string id)
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

	public async Task<List<string>> UpdateTeacher(Teacher teacher)
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

	public async Task<List<string>> UpdateTeachers(List<Teacher> teachers, bool isAdmin, bool isInactive)
	{
		List<string> errorList = new();
		try
		{
			if (isInactive)
			{
				var responseInactive = await ApiRequests.GetInactiveUsersAsync();
				DatabaseService.InactiveUsers = new(responseInactive.Values[0]);
			}

			var responseStatus = await UpdateTeachersStatus(teachers, isAdmin, isInactive, errorList);
			errorList = responseStatus.ToList();

			if (isInactive) await FetchTeachers();
			if (isAdmin) await DatabaseService.FetchAdmins();

			NotificationService.NotifySuccess($"Status was updated for {teachers.Count} teacher(s)");
		}
		catch (Exception e)
		{
			NotificationService.HandleError(e);
		}

		return errorList;
	}

	public async Task<List<string>> UpdateTeachersStatus(List<Teacher> teachers, bool isAdmin,
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
}

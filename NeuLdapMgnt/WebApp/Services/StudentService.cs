using Microsoft.AspNetCore.Components;
using NeuLdapMgnt.Models;
using NeuLdapMgnt.WebApp.Requests;

namespace NeuLdapMgnt.WebApp.Services;

public class StudentService
{
	[Inject] private ApiRequests ApiRequests { get; set; }

	[Inject] private LocalDbService DatabaseService { get; set; }

	[Inject] private NotificationService NotificationService { get; set; }

	public List<Student> Students { get; set; } = new();

	public StudentService(ApiRequests apiRequests, LocalDbService localDbService,
		NotificationService notificationService)
	{
		ApiRequests = apiRequests;
		DatabaseService = localDbService;
		NotificationService = notificationService;
	}

	public async Task FetchStudentsAsync()
	{
		try
		{
			await DatabaseService.FetchInactiveUsersAsync();

			var response = await ApiRequests.GetStudentsAsync();
			if (response.IsSuccess())
			{
				Students = new(response.Values.Where(x =>
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

	public async Task<Student?> FetchStudentAsync(long id)
	{
		try
		{
			var response = await ApiRequests.GetStudentAsync(id.ToString());
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

	public async Task AddStudentAsync(Student student)
	{
		try
		{
			var response = await ApiRequests.AddStudentAsync(student);
			if (response.IsSuccess())
			{
				NotificationService.NotifySuccess($"{student.FullName} was added!");
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

	public async Task<List<string>> UpdateStudentAsync(Student student)
	{
		List<string> errorList = new();
		try
		{
			var response = await ApiRequests.UpdateStudentAsync(student.Id.ToString(), student);

			if (response.IsSuccess())
			{
				NotificationService.NotifySuccess($"{student.FullName} was updated!");
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

	public async Task<List<string>> UpdateStudentsAsync(List<Student> students, string newClass, bool isInactive)
	{
		List<string> errorList = new();
		try
		{
			if (isInactive)
			{
				var response = await ApiRequests.GetInactiveUsersAsync();
				DatabaseService.InactiveUsers = new(response.Values[0]);
			}

			foreach (var student in students)
			{
				if (isInactive && !DatabaseService.InactiveUsers.Contains(student.Id.ToString()))
				{
					var response = await ApiRequests.DeactivateUserAsync(student.Id.ToString());
					if (response.IsFailure())
					{
						errorList.AddRange(response.Errors);
					}
				}

				if (!string.IsNullOrEmpty(newClass) && !student.Class.Equals(newClass))
				{
					student.Class = newClass;
					var response = await ApiRequests.UpdateStudentAsync(student.Id.ToString(), student);
					if (response.IsFailure())
					{
						errorList.AddRange(response.Errors);
					}
				}
			}

			if (!string.IsNullOrEmpty(newClass))
			{
				NotificationService.NotifySuccess($"Class was set to [{newClass}] for {students.Count} student(s)");
			}

			if (isInactive)
			{
				NotificationService.NotifySuccess($"Status was set to [Inactive] for {students.Count} student(s)");
			}
		}
		catch (Exception e)
		{
			NotificationService.HandleError(e);
		}

		return errorList;
	}
}

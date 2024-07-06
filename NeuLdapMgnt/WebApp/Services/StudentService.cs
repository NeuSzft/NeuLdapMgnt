using Microsoft.AspNetCore.Components;
using NeuLdapMgnt.Models;
using NeuLdapMgnt.WebApp.Requests;

namespace NeuLdapMgnt.WebApp.Services;

/// <summary>
/// Service for managing students.
/// </summary>
public class StudentService
{
	[Inject] private ApiRequests ApiRequests { get; set; }

	[Inject] private LocalDbService DatabaseService { get; set; }

	[Inject] private NotificationService NotificationService { get; set; }

	public List<Student> Students { get; set; } = new();

	/// <summary>
	/// Constructor for StudentService.
	/// </summary>
	/// <param name="apiRequests">Instance of ApiRequests for API communication.</param>
	/// <param name="localDbService">Instance of LocalDbService for local database operations.</param>
	/// <param name="notificationService">Instance of NotificationService for handling notifications.</param>
	public StudentService(ApiRequests apiRequests, LocalDbService localDbService, NotificationService notificationService)
	{
		ApiRequests = apiRequests;
		DatabaseService = localDbService;
		NotificationService = notificationService;
	}

	/// <summary>
	/// Fetches students asynchronously.
	/// </summary>
	public async Task FetchStudentsAsync()
	{
		try
		{
			var response = await ApiRequests.GetStudentsAsync();
			if (response.IsSuccess())
			{
				Students = new(response.Values);
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
	/// Fetches a student asynchronously by ID.
	/// </summary>
	/// <param name="id">The ID of the student to fetch.</param>
	/// <returns>The fetched student, or null if not found.</returns>
	public async Task<Student?> FetchStudentAsync(string id)
	{
		try
		{
			var response = await ApiRequests.GetStudentAsync(id);
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
	/// Adds a new student asynchronously.
	/// </summary>
	/// <param name="student">The student to add.</param>
	public async Task AddStudentAsync(Student student)
	{
		try
		{
			var response = await ApiRequests.AddStudentAsync(student, !string.IsNullOrWhiteSpace(student.Password));
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

	/// <summary>
	/// Updates a student asynchronously.
	/// </summary>
	/// <param name="student">The student to update.</param>
	/// <returns>A list of error messages encountered during the update.</returns>
	public async Task<List<string>> UpdateStudentAsync(Student student)
	{
		List<string> errorList = new();
		try
		{
			var response = await ApiRequests.UpdateStudentAsync(student.Id.ToString(), student, !string.IsNullOrWhiteSpace(student.Password));

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

	/// <summary>
	/// Updates multiple students asynchronously.
	/// </summary>
	/// <param name="students">The list of students to update.</param>
	/// <param name="newClass">The new class for the students.</param>
	/// <param name="isInactive">Flag indicating whether the students should be set to inactive.</param>
	/// <returns>A list of error messages encountered during the update.</returns>
	public async Task<List<string>> UpdateStudentsAsync(List<Student> students, string newClass, bool isInactive)
	{
		List<string> errorList = new();
		try
		{
			foreach (var student in students)
			{
				if (isInactive && !student.IsInactive)
				{
					student.IsInactive = true;
				}

				if (!string.IsNullOrEmpty(newClass) && !student.Class.Equals(newClass))
				{
					student.Class = newClass;
				}

				var response = await ApiRequests.UpdateStudentAsync(student.Id.ToString(), student, !string.IsNullOrWhiteSpace(student.Password));
				if (response.IsFailure())
				{
					errorList.AddRange(response.Errors);
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

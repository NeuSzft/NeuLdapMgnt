using NeuLdapMgnt.WebApp.Model;
using NeuLdapMgnt.Models;
using BlazorBootstrap;
using NeuLdapMgnt.WebApp.ComponentOptions;
using NeuLdapMgnt.WebApp.Components;
using NeuLdapMgnt.WebApp.Requests;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

namespace NeuLdapMgnt.WebApp.Services
{
	public class LocalDbService
	{
		[Inject]
		private ApiRequests ApiRequests { get; set; }

		[Inject]
		private ModalService ModalService { get; set; }

		[Inject]
		private ToastService ToastService { get; set; }

		public List<Student> Students { get; set; } = new();

		public List<Teacher> Teachers { get; set; } = new();

		public List<string> Admins { get; set; } = new();

		public List<string> Classes { get; set; } = new();

		public LocalDbService(ApiRequests apiRequests, ModalService modalService, ToastService toastService)
		{
			ApiRequests = apiRequests;
			ModalService = modalService;
			ToastService = toastService;
		}

		public async Task FetchClasses()
		{
			try
			{
				Classes.Clear();

				var response = await ApiRequests.GetClassesAsync();
				if (response.IsSuccess())
				{
					Classes = response.Values.SelectMany(x => x).OrderBy(x => x).ToList();
				}
				if (response.Errors.Any())
				{
					ToastService.Notify(ToastMessages.Error(response.GetError()));
				}
			}
			catch (Exception e)
			{
				string message = e is HttpRequestException re ? re.GetErrorMessage() : e.Message;
				ToastService.Notify(ToastMessages.Error(message));
			}
		}

		public async Task FetchStudents()
		{
			try
			{
				Students.Clear();

				var response = await ApiRequests.GetStudentsAsync();
				if (response.IsSuccess())
				{
					Students = new(response.Values);
				}

				if (response.Errors.Any())
				{
					ToastService.Notify(ToastMessages.Error(response.GetError()));
				}
			}
			catch (Exception e)
			{
				string message = e is HttpRequestException re ? re.GetErrorMessage() : e.Message;
				ToastService.Notify(ToastMessages.Error(message));
			}
		}

		public async Task FetchTeachers()
		{
			try
			{
				Teachers.Clear();

				var responseTeachers = await ApiRequests.GetTeachersAsync();
				if (responseTeachers.IsSuccess())
				{
					Teachers = new(responseTeachers.Values);
				}

				if (responseTeachers.Errors.Any())
				{
					ToastService.Notify(ToastMessages.Error(responseTeachers.GetError()));
				}
			}
			catch (HttpRequestException httpError)
			{
				if (httpError.StatusCode is HttpStatusCode.BadRequest)
				{
					ToastService.Notify(ToastMessages.NoConnection());
				}
				else
				{
					ToastService.Notify(ToastMessages.Error(httpError.Message));
				}
			}
			catch (Exception e)
			{
				ToastService.Notify(ToastMessages.Error(e.Message));
			}
		}

		public async Task FetchAdmins()
		{
			try
			{
				Admins.Clear();

				var responseAdmins = await ApiRequests.GetAdminsAsync();
				if (responseAdmins.IsSuccess())
				{
					Admins = new(responseAdmins.Values[0]);
				}

				if (responseAdmins.Errors.Any())
				{
					ToastService.Notify(ToastMessages.Error(responseAdmins.GetError()));
				}
			}
			catch (HttpRequestException httpError)
			{
				if (httpError.StatusCode is HttpStatusCode.BadRequest)
				{
					ToastService.Notify(ToastMessages.NoConnection());
				}
				else
				{
					ToastService.Notify(ToastMessages.Error(httpError.Message));
				}
			}
			catch (Exception e)
			{
				ToastService.Notify(ToastMessages.Error(e.Message));
			}
		}

		public async Task AddStudent(Student student)
		{
			try
			{
				var response = await ApiRequests.AddStudentAsync(student);

				if (response.IsSuccess())
				{
					ToastService.Notify(ToastMessages.Success($"{student.FullName} ({student.Id}) was added!"));
				}

			}
			catch (Exception e)
			{
				string message = e is HttpRequestException re ? re.GetErrorMessage() : e.Message;
				ToastService.Notify(ToastMessages.Error(message));
			}
		}

		public async Task UpdateStudentClassInBulk(IEnumerable<Student> students, string newClass)
		{
			try
			{
				foreach (var student in students)
				{
					if (student.Class.Equals(newClass))
					{
						continue;
					}

					student.Class = newClass;
					await ApiRequests.UpdateStudentAsync(student.Id, student);
				}
				ToastService.Notify(ToastMessages.Success("Selected students were updated."));
			}
			catch (Exception e)
			{
				string message = e is HttpRequestException re ? re.GetErrorMessage() : e.Message;
				ToastService.Notify(ToastMessages.Error(message));
			}
		}

		public async Task DeleteStudentsInBulk(IEnumerable<Student> students)
		{
			try
			{
				foreach (var student in students)
				{
					await ApiRequests.DeleteStudentAsync(student.Id);
				}
				ToastService.Notify(ToastMessages.Success("Selected students were deleted!"));
			}
			catch (Exception e)
			{
				string message = e is HttpRequestException re ? re.GetErrorMessage() : e.Message;
				ToastService.Notify(ToastMessages.Error(message));
			}
		}

		public async Task DeleteTeachersInBulk(IEnumerable<Teacher> teachers)
		{
			try
			{
				foreach (var teacher in teachers)
				{
					await ApiRequests.DeleteTeacherAsync(teacher.Id);
				}
				ToastService.Notify(ToastMessages.Success("Selected teachers were deleted!"));
			}
			catch (Exception e)
			{
				string message = e is HttpRequestException re ? re.GetErrorMessage() : e.Message;
				ToastService.Notify(ToastMessages.Error(message));
			}
		}
	}
}

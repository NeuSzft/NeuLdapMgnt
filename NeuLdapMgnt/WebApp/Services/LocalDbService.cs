using Microsoft.AspNetCore.Components;
using BlazorBootstrap;
using NeuLdapMgnt.Models;
using NeuLdapMgnt.WebApp.ComponentOptions;
using NeuLdapMgnt.WebApp.Requests;

namespace NeuLdapMgnt.WebApp.Services
{
	public class LocalDbService
	{
		[Inject]
		private ApiRequests ApiRequests { get; set; }

		[Inject]
		private ToastService ToastService { get; set; }

		public List<Student> Students { get; set; } = new();

		public List<Teacher> Teachers { get; set; } = new();

		public List<string> Admins { get; set; } = new();

		public List<string> Classes { get; set; } = new();

		public List<string> InactiveUsers { get; set; } = new();

		public LocalDbService(ApiRequests apiRequests, ToastService toastService)
		{
			ApiRequests = apiRequests;
			ToastService = toastService;
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

		public async Task<Student?> FetchStudent(long id)
		{
			try
			{
				var response = await ApiRequests.GetStudentAsync(id);
				if (response.IsSuccess())
				{
					return response.Values[0];
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
			return null;
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

		public async Task UpdateStudentsInBulk(IEnumerable<Student> students, string newClass, bool isInactive)
		{
			try
			{
				if (isInactive)
				{
					var response = await ApiRequests.GetInactiveUsersAsync();
					InactiveUsers = new(response.Values[0]);
				}

				foreach (var student in students)
				{
					if (isInactive && !InactiveUsers.Contains(student.Id.ToString()))
					{
						await ApiRequests.DeactivateUserAsync(student.Id.ToString());
					}

					if (!string.IsNullOrEmpty(newClass) && !student.Class.Equals(newClass))
					{
						student.Class = newClass;
						await ApiRequests.UpdateStudentAsync(student.Id, student);
					}
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
				foreach (var student in students.Select(x => x.Id))
				{
					await ApiRequests.DeleteStudentAsync(student);

					if (InactiveUsers.Contains(student.ToString()))
					{
						await ActivateUser(student.ToString());
					}
				}
				ToastService.Notify(ToastMessages.Success("Selected students were deleted!"));
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

				var response = await ApiRequests.GetTeachersAsync();
				if (response.IsSuccess())
				{
					Teachers = new(response.Values);
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

		public async Task<Teacher?> FetchTeacher(string id)
		{
			try
			{
				var response = await ApiRequests.GetTeacherAsync(id);
				if (response.IsSuccess())
				{
					return response.Values[0];
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
			return null;
		}

		public async Task UpdateTeachersInBulk(IEnumerable<Teacher> teachers, bool isAdmin, bool isInactive)
		{
			try
			{
				if (isInactive)
				{
					var response = await ApiRequests.GetInactiveUsersAsync();
					InactiveUsers = new(response.Values[0]);
				}

				foreach (var teacher in teachers)
				{
					if (isAdmin && !Admins.Contains(teacher.Id))
					{
						await ApiRequests.AddAdminAsync(teacher.Id);
					}

					if (isInactive && !InactiveUsers.Contains(teacher.Id))
					{
						await ApiRequests.DeactivateUserAsync(teacher.Id);
					}

					await ApiRequests.UpdateTeacherAsync(teacher.Id, teacher);
				}

				if (isInactive)
				{
					await FetchTeachers();
				}

				if (isAdmin)
				{
					await FetchAdmins();
				}

				ToastService.Notify(ToastMessages.Success("Selected teachers were updated."));
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
				await FetchAdmins();
				await FetchInactiveUsers();

				foreach (var teacher in teachers.Select(x => x.Id))
				{
					await ApiRequests.DeleteTeacherAsync(teacher);

					if (Admins.Contains(teacher))
					{
						await ApiRequests.DeleteAdminAsync(teacher);
					}

					if (InactiveUsers.Contains(teacher))
					{
						await ActivateUser(teacher);
					}
				}
				ToastService.Notify(ToastMessages.Success("Selected teachers were deleted!"));
			}
			catch (Exception e)
			{
				string message = e is HttpRequestException re ? re.GetErrorMessage() : e.Message;
				ToastService.Notify(ToastMessages.Error(message));
			}
		}

		public async Task FetchAdmins()
		{
			try
			{
				Admins.Clear();

				var response = await ApiRequests.GetAdminsAsync();
				if (response.IsSuccess())
				{
					Admins = new(response.Values[0]);
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

		public async Task FetchInactiveUsers()
		{
			try
			{
				InactiveUsers.Clear();

				var response = await ApiRequests.GetInactiveUsersAsync();
				if (response.IsSuccess())
				{
					InactiveUsers = new(response.Values[0]);
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

		public async Task FetchClasses()
		{
			try
			{
				Classes.Clear();

				var response = await ApiRequests.GetClassesAsync();
				if (response.IsSuccess())
				{
					Classes = response.Values.SelectMany(x => x).OrderBy(x => Utils.GetClassOrderValue(x)).ToList();
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

		public async Task DeactivateUser(string id)
		{
			try
			{
				var response = await ApiRequests.DeactivateUserAsync(id);
				if (response.IsSuccess())
				{
					await FetchInactiveUsers();
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

		public async Task ActivateUser(string id)
		{
			try
			{
				var response = await ApiRequests.ActivateUserAsync(id);
				if (response.IsSuccess())
				{
					await FetchInactiveUsers();
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
	}
}

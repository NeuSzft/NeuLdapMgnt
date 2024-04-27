using Microsoft.AspNetCore.Components;
using NeuLdapMgnt.WebApp.Requests;

namespace NeuLdapMgnt.WebApp.Services
{
	public class LocalDbService
	{
		[Inject]
		private ApiRequests ApiRequests { get; set; }

		[Inject]
		private NotificationService NotificationService { get; set; }

		public List<string> Admins { get; set; } = new();

		public List<string> Classes { get; set; } = new();

		public List<string> InactiveUsers { get; set; } = new();

		public LocalDbService(ApiRequests apiRequests, NotificationService notificationService)
		{
			ApiRequests = apiRequests;
			NotificationService = notificationService;
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
					NotificationService.NotifyError(response.GetError());
				}
			}
			catch (Exception e)
			{
				NotificationService.HandleError(e);
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
					NotificationService.NotifyError(response.GetError());
				}
			}
			catch (Exception e)
			{
				NotificationService.HandleError(e);
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

		public async Task DeactivateUser(string id)
		{
			try
			{
				var response = await ApiRequests.DeactivateUserAsync(id);
				if (response.IsSuccess())
				{
					InactiveUsers.Remove(id);
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

		public async Task<List<string>> DeactivateUsers(List<string> ids)
		{
			List<string> errorList = new();
			try
			{
				await FetchInactiveUsers();
				foreach (var id in ids)
				{
					var response = await ApiRequests.DeactivateUserAsync(id);
					if (response.IsSuccess())
					{
						InactiveUsers.Add(id);
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

		public async Task<List<string>> ActivateUsers(List<string> ids)
		{
			List<string> errorList = new();
			try
			{
				await FetchInactiveUsers();
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
	}
}

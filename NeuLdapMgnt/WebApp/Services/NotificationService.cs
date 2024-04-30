using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using NeuLdapMgnt.WebApp.ComponentOptions;

namespace NeuLdapMgnt.WebApp.Services;

/// <summary>
/// Service for displaying notifications using toast messages.
/// </summary>
public class NotificationService
{
	[Inject] private ToastService ToastService { get; set; }

	/// <summary>
	/// Constructor for NotificationService.
	/// </summary>
	/// <param name="toastService">Instance of ToastService for displaying toast messages.</param>
	public NotificationService(ToastService toastService)
	{
		ToastService = toastService;
	}

	/// <summary>
	/// Handles errors by displaying an error toast message.
	/// </summary>
	/// <param name="exception">The exception to handle.</param>
	public void HandleError(Exception exception)
	{
		string message = exception is HttpRequestException re ? re.GetErrorMessage() : exception.Message;
		NotifyError(message);
	}

	/// <summary>
	/// Displays an error toast message.
	/// </summary>
	/// <param name="message">The error message to display.</param>
	public void NotifyError(string message)
	{
		ToastService.Notify(ToastMessages.Error(message));
	}

	/// <summary>
	/// Displays a success toast message.
	/// </summary>
	/// <param name="message">The success message to display.</param>
	public void NotifySuccess(string message)
	{
		ToastService.Notify(ToastMessages.Success(message));
	}

	/// <summary>
	/// Displays a toast message indicating a timeout.
	/// </summary>
	/// <param name="minutesLeft">The number of minutes left until timeout.</param>
	public void NotifyTimeout(int minutesLeft)
	{
		ToastService.Notify(ToastMessages.Dark($"After {minutesLeft} minute{(minutesLeft <= 1 ? "s" : string.Empty)} you will be logged out because of inactivity!"));
	}
}

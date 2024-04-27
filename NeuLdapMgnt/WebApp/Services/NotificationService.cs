﻿using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using NeuLdapMgnt.WebApp.ComponentOptions;

namespace NeuLdapMgnt.WebApp.Services;

public class NotificationService
{
	[Inject]
	private ToastService ToastService { get; set; }

	public NotificationService(ToastService toastService)
	{
		ToastService = toastService;
	}

	public void HandleError(Exception e)
	{
		string message = e is HttpRequestException re ? re.GetErrorMessage() : e.Message;
		NotifyError(message);
	}

	public void NotifyError(string message)
	{
		ToastService.Notify(ToastMessages.Error(message));
	}

	public void NotifySuccess(string message)
	{
		ToastService.Notify(ToastMessages.Success(message));
	}
}
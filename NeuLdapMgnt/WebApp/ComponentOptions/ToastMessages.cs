﻿using BlazorBootstrap;

namespace NeuLdapMgnt.WebApp.ComponentOptions
{
	public static class ToastMessages
	{
		public static ToastMessage WelcomeBack(string name)
		{
			return new()
			{
				Type = ToastType.Secondary,
				AutoHide = true,
				Message = $"Welcome back {name}!"
			};
		}

		public static ToastMessage InvalidCredentials()
		{
			return new()
			{
				Type = ToastType.Danger,
				AutoHide = true,
				Message = "Incorrect username or password!"
			};
		}

		public static ToastMessage Error(string message = "Error!")
		{
			return new()
			{
				Type = ToastType.Danger,
				AutoHide = true,
				Message = message
			};
		}

		public static ToastMessage Secondary(string message)
		{
			return new()
			{
				Type = ToastType.Secondary,
				AutoHide = true,
				Message = message
			};
		}

		public static ToastMessage Success(string message = "Successful operation!")
		{
			return new()
			{
				Type = ToastType.Success,
				AutoHide = true,
				Message = message
			};
		}

		public static ToastMessage Danger(string message)
		{
			return new()
			{
				Type = ToastType.Danger,
				AutoHide = true,
				Message = message
			};
		}

		public static ToastMessage Warning(string message, bool autoHide = false)
		{
			return new()
			{
				Type = ToastType.Warning,
				AutoHide = autoHide,
				Message = message
			};
		}

		public static ToastMessage Light(string message)
		{
			return new()
			{
				Type = ToastType.Light,
				AutoHide = true,
				Message = message
			};
		}

		public static ToastMessage Dark(string message)
		{
			return new()
			{
				Type = ToastType.Dark,
				AutoHide = true,
				Message = message
			};
		}
	}
}

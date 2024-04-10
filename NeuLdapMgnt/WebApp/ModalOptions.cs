using BlazorBootstrap;

namespace NeuLdapMgnt.WebApp
{
	public static class ModalOptions
	{
		public static ModalOption NoConnection(string title = "No Connection", string message = "Couldn't connect to the API")
		{
			return new ModalOption()
			{
				Title = title,
				Message = message,
				Type = ModalType.Danger,
				ShowFooterButton = false
			};
		}

		public static ModalOption Error(string error = "Something went wrong")
		{
			return new ModalOption()
			{
				Title = "Error",
				Message = error,
				Type = ModalType.Danger,
				ShowFooterButton = false
			};
		}

		public static ModalOption Danger(string title, string message)
		{
			return new ModalOption()
			{
				Title = title,
				Message = message,
				Type = ModalType.Danger,
				ShowFooterButton = false
			};
		}

		public static ModalOption Success(string title, string message = "Successful operation")
		{
			return new ModalOption()
			{
				Title = title,
				Message = message,
				Type = ModalType.Success,
				ShowFooterButton = false
			};
		}

		public static ModalOption Warning(string title, string message)
		{
			return new ModalOption()
			{
				Title = title,
				Message = message,
				Type = ModalType.Warning,
				ShowFooterButton = false
			};
		}

		public static ModalOption Info(string title, string message)
		{
			return new ModalOption()
			{
				Title = title,
				Message = message,
				Type = ModalType.Info,
				ShowFooterButton = false
			};
		}

		public static ModalOption Light(string title, string message)
		{
			return new ModalOption()
			{
				Title = title,
				Message = message,
				Type = ModalType.Light,
				ShowFooterButton = false
			};
		}

		public static ModalOption Dark(string title, string message)
		{
			return new ModalOption()
			{
				Title = title,
				Message = message,
				Type = ModalType.Dark,
				ShowFooterButton = false
			};
		}
	}
}

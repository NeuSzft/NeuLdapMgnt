using BlazorBootstrap;

namespace NeuLdapMgnt.WebApp.ComponentOptions
{
	public static class DialogOptions
	{
		public static ConfirmDialogOptions Edit(DialogSize size = DialogSize.Large)
		{
			return new()
			{
				YesButtonText = "Edit",
				YesButtonColor = ButtonColor.Warning,
				NoButtonText = "Cancel",
				NoButtonColor = ButtonColor.Secondary,
				IsScrollable = true,
				Size = size
			};
		}

		public static ConfirmDialogOptions Confirm(DialogSize size = DialogSize.Regular)
		{
			return new()
			{
				YesButtonText = "Yes",
				YesButtonColor = ButtonColor.Success,
				NoButtonText = "No",
				NoButtonColor = ButtonColor.Secondary,
				IsScrollable = true,
				Size = size
			};
		}

		public static ConfirmDialogOptions Delete(DialogSize size = DialogSize.Regular)
		{
			return new()
			{
				YesButtonText = "Yes",
				YesButtonColor = ButtonColor.Danger,
				NoButtonText = "No",
				NoButtonColor = ButtonColor.Secondary,
				IsScrollable = true,
				Size = size
			};
		}
	}
}

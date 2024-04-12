using BlazorBootstrap;

namespace NeuLdapMgnt.WebApp.ComponentOptions
{
	public static class DialogOptions
	{
		public static ConfirmDialogOptions Edit(DialogSize size = DialogSize.Large)
		{
			return new()
			{
				YesButtonText = "Yes",
				YesButtonColor = ButtonColor.Success,
				NoButtonText = "Cancel",
				NoButtonColor = ButtonColor.Secondary,
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
				Size = size
			};
		}

		public static ConfirmDialogOptions Delete(DialogSize size = DialogSize.Regular)
		{
			return new()
			{
				YesButtonText = "DELETE",
				YesButtonColor = ButtonColor.Danger,
				NoButtonText = "Cancel",
				NoButtonColor = ButtonColor.Success,
				IsScrollable = true,
				Size = size
			};
		}
	}
}

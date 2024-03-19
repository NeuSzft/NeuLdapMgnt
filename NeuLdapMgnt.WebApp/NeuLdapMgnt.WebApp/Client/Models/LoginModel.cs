using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace NeuLdapMgnt.WebApp.Client.Models
{
	public class LoginModel
	{
		[Required]
		public string Username { get; set; } = null!;
		[Required, PasswordPropertyText]
		public string Password { get; set; } = null!;
	}
}

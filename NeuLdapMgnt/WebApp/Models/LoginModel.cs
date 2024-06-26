﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.WebApp.Models
{
	public class LoginModel
	{
		[Required]
		public string Username { get; set; } = null!;
		[Required, PasswordPropertyText]
		public string Password { get; set; } = null!;
	}
}

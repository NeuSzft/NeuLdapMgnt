using System;
using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models
{
	public class Teacher : Person
	{
		[Required]
		[LdapAttribute("uid")]
		public override long Id { get; set; }

		[Required, Range(4000, 5999)]
		[LdapAttribute("uid")]
		public override int Uid { get; set; } = 4000;

		[Required, Range(4000, 5999)]
		[LdapAttribute("gidNumber")]
		public override int Gid { get; set; } = 4000;
		public override string Username { get; set; } = string.Empty;
		public override string FirstName { get; set; } = string.Empty;
		public override string LastName { get; set; } = string.Empty;
		public override string? MiddleName { get; set; } = string.Empty;
		public override string Email { get; set; } = string.Empty;
		public override string HomeDirectory { get; set; } = string.Empty;
		public override string Password { get; set; } = string.Empty;
		public override string FullName { get; set; } = string.Empty;
	}
}

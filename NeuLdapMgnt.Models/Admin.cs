using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuLdapMgnt.Models
{
	public class Admin : Person
	{
		[Required]
		[LdapAttribute("uid")]
		public override long Id { get; set; }

		[Required, Range(1000, 3999)]
		[LdapAttribute("uid")]
		public override int Uid { get; set; } = 1000;

		[Required, Range(1000, 3999)]
		[LdapAttribute("gidNumber")]
		public override int Gid { get; set; } = 1000;
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

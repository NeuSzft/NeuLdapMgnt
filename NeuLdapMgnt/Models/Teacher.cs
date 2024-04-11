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
	}
}

using System;
using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models
{
	public class Teacher : Person
	{
		[Required, StringLength(11), RegularExpression("^[7][0-9]{10}$")]
		[LdapAttribute("uid")]
		public override string Id { get; set; }

		[Required, Range(4000, 5999)]
		[LdapAttribute("uid")]
		public override int Uid { get; set; }

		[Required, Range(4000, 5999)]
		[LdapAttribute("gidNumber")]
		public override int Gid { get; set; }

		public Teacher(string id, int uid, int gid, string firstName, string lastName, string? middleName = null)
			: base(id, uid, gid, firstName, lastName, middleName)
		{
			Id = id;
			Password = GeneratePassword();
		}

		protected override string GeneratePassword() => string.Join('.', FirstName, LastName, Uid);
	}
}

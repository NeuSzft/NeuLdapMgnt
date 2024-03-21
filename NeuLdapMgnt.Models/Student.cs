using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models
{
	public class Student : Person
	{
		[Required, StringLength(11), RegularExpression("^[7][0-9]{10}$")]
		[LdapAttribute("uid")]
		public override string Id { get; set; }

		[Required, Range(6000, int.MaxValue)]
		[LdapAttribute("uidNumber")]
		public override int Uid { get; set; }

		[Required, Range(6000, int.MaxValue)]
		[LdapAttribute("gidNumber")]
		public override int Gid { get; set; }

		[Required]
		[LdapAttribute("roomNumber")]
		public string Class { get; set; }

		[Required]
		[LdapAttribute("homeDirectory")]
		public override string HomeDirectory => $"/home/{Username}";

		public Student(string id, int uid, int gid, string firstName, string lastName, string @class, string? middleName = null)
			: base(id, uid, gid, firstName, lastName, middleName)
		{
			Id = id;
			Uid = uid;
			Gid = gid;
			Class = @class;
		}

		protected override string GeneratePassword() => string.Join('.', FirstName, LastName, Uid);
	}
}

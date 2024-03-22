using System;
using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models
{
	public sealed class Student : Person, IEquatable<Student>
	{
		[Required, StringLength(11, ErrorMessage = "The OM field length must be 11."), RegularExpression("^[7][0-9]{10}$", ErrorMessage = "The OM field is not a valid OM.")]
		[LdapAttribute("uid")]
		public override string Id { get; set; }

		[Required, Range(6000, 9999)]
		public override int Uid { get; set; }

		[Required, Range(6000, 9999)]
		public override int Gid { get; set; }

		[Required]
		[LdapAttribute("roomNumber")]
		public string Class { get; set; }

		public Student(string id, int uid, int gid, string firstName, string lastName, string @class, string? middleName = null)
			: base(id, uid, gid, firstName, lastName, middleName)
		{
			Id = id;
			Class = @class;
			Password = GeneratePassword();
		}

		protected override string GeneratePassword() => string.Join('.', FirstName, LastName, Uid);

		public bool Equals(Student? other)
		{
			if (other == null) return false;

			return Id == other.Id
				&& FirstName == other.FirstName
				&& MiddleName == other.MiddleName
				&& LastName == other.LastName
				&& Class == other.Class
				&& Username == other.Username
				&& Uid == other.Uid
				&& Gid == other.Gid
				&& Email == other.Email
				&& HomeDirectory == other.HomeDirectory
				&& Password == other.Password;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Id, FirstName, LastName, Class, Username);
		}
	}
}

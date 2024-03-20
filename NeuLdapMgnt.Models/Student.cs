using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace NeuLdapMgnt.Models
{
	public class Student
	{
		[Required, StringLength(11), RegularExpression("^[7][0-9]{10}$")]
		public string OM { get; set; }

		[Required, Range(6000, int.MaxValue)]
		public int Uid { get; set; }

		[Required, Range(6000, int.MaxValue)]
		public int Gid { get; set; }

		[Required]
		public string Username { get; set; }

		[Required, MinLength(3)]
		public string FirstName { get; set; }

		[AllowNull]
		public string? MiddleName { get; set; }

		[Required, MinLength(3)]
		public string LastName { get; set; }

		[Required]
		public string Class { get; set; }


		[Required, PasswordPropertyText, MinLength(8)]
		public string Password { get; set; }

		public Student(string oM, int uid, int gid, string firstName, string lastName, string @class, string? middleName = null)
		{
			OM = oM;
			Uid = uid;
			Gid = gid;
			FirstName = firstName;
			LastName = lastName;
			Class = @class;
			MiddleName = middleName;
			Password = GeneratePassword();
			Username = GenerateUsername();
		}

		public string GetFullName() => string.Join(' ', FirstName, MiddleName, LastName);
		public string GetEmail() => $"{OM}@neu.ldap.hu";
		public string GetHomeDirectory() => $"/home/students/{OM}";
		private string GeneratePassword() => string.Join('.', FirstName, LastName, Uid);
		private string GenerateUsername() => string.Join("", FirstName[..3], LastName[..3]).ToLower();
	}
}

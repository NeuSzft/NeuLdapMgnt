using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace NeuLdapMgnt.Models
{
	public abstract class Person
	{
		private string mail;
		private string username;
		private string fullName;

		[Required]
		[LdapAttribute("uid")]
		public abstract string Id { get; set; }

		[Required]
		[LdapAttribute("uidNumber")]
		public abstract int Uid { get; set; }

		[Required]
		[LdapAttribute("gidNumber")]
		public abstract int Gid { get; set; }

		[Required]
		[LdapAttribute("cn")]
		public string Username
		{
			get => string.Join("", FirstName[..3], LastName[..3]).ToLower();
			set => username = value;
		}

		[Required, MinLength(3)]
		[LdapAttribute("givenName")]
		public string FirstName { get; set; }

		[Required, MinLength(3)]
		[LdapAttribute("sn")]
		public string LastName { get; set; }

		[AllowNull]
		public string? MiddleName { get; set; }

		[Required, EmailAddress]
		[LdapAttribute("mail")]
		public string Mail
		{
			get => $"{Id}@neu.ldap.hu";
			set => mail = value;
		}

		[Required]
		[LdapAttribute("homeDirectory")]
		public abstract string HomeDirectory { get; }

		[Required, PasswordPropertyText, MinLength(8)]
		[LdapAttribute("userPassword")]
		public string Password { get; set; }

		[LdapAttribute("displayName")]
		public string FullName
		{
			get => string.Join(' ', FirstName, MiddleName, LastName);
			set => fullName = value;
		}

		protected Person(string id, int uid, int gid, string firstName, string lastName, string? middleName = null)
		{
			Id = id;
			Uid = uid;
			Gid = gid;
			FirstName = firstName;
			LastName = lastName;
			MiddleName = middleName;
		}

		protected abstract string GeneratePassword();
	}
}

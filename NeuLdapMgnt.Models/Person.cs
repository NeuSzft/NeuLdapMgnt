using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace NeuLdapMgnt.Models
{
    [LdapObjectClasses("inetOrgPerson", "posixAccount")]
	public abstract class Person
	{
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
		public string Username { get; set; }

		[Required, MinLength(3, ErrorMessage = "The field First name must be a string with a minimum length of '3'.")]
		[LdapAttribute("givenName")]
		public string FirstName { get; set; }

		[Required, MinLength(3, ErrorMessage = "The field Last name must be a string with a minimum length of '3'.")]
		[LdapAttribute("sn")]
		public string LastName { get; set; }

		[AllowNull]
		public string? MiddleName { get; set; }

		[Required, EmailAddress]
		[LdapAttribute("mail")]
		public string Email { get; set; }

		[Required]
		[LdapAttribute("homeDirectory")]
		public string HomeDirectory { get; set; }

		[Required, PasswordPropertyText, MinLength(8)]
		[LdapAttribute("userPassword")]
		public string Password { get; set; } = null!;

		[LdapAttribute("displayName")]
		public string FullName { get; set; }

		protected Person(string id, int uid, int gid, string firstName, string lastName, string? middleName = null)
		{
			Id = id;
			Uid = uid;
			Gid = gid;
			FirstName = firstName;
			LastName = lastName;
			MiddleName = middleName;
			Username = string.Join("", FirstName[..3], LastName[..3]).ToLower();
			Email = $"{Id}@neu.ldap.hu";
			HomeDirectory = $"/home/{Username}";
			FullName = string.Join(' ', FirstName, MiddleName, LastName);
		}

		protected abstract string GeneratePassword();
	}
}

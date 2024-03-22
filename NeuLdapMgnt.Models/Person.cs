using System;
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
		public abstract long Id { get; set; }

		[Required]
		[LdapAttribute("uidNumber")]
		public abstract int Uid { get; set; }

		[Required]
		[LdapAttribute("gidNumber")]
		public abstract int Gid { get; set; }

		[Required]
		[LdapAttribute("cn")]
		public abstract string Username { get; set; }

		[Required, MinLength(3, ErrorMessage = "The field First name must be a string with a minimum length of '3'.")]
		[LdapAttribute("givenName")]
		public abstract string FirstName { get; set; }

		[Required, MinLength(3, ErrorMessage = "The field Last name must be a string with a minimum length of '3'.")]
		[LdapAttribute("sn")]
		public abstract string LastName { get; set; }

		[AllowNull]
		public abstract string? MiddleName { get; set; }

		[Required, EmailAddress]
		[LdapAttribute("mail")]
		public abstract string Email { get; set; }

		[Required]
		[LdapAttribute("homeDirectory")]
		public abstract string HomeDirectory { get; set; }

		[Required, PasswordPropertyText, MinLength(8)]
		[LdapAttribute("userPassword")]
		public abstract string Password { get; set; }

		[LdapAttribute("displayName")]
		public abstract string FullName { get; set; }

		public string GetFullName() => string.Join(" ", FirstName, MiddleName, LastName);
	}
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace NeuLdapMgnt.Models
{
	[LdapObjectClasses("inetOrgPerson", "posixAccount")]
	public abstract class Person
	{
		[Required]
		[JsonPropertyName("id")]
		[LdapAttribute("uid")]
		public virtual long Id { get; set; }

		[Required]
		[JsonRequired, JsonPropertyName("uid")]
		[LdapAttribute("uidNumber")]
		public virtual int Uid { get; set; }

		[Required]
		[JsonRequired, JsonPropertyName("gid")]
		[LdapAttribute("gidNumber")]
		public virtual int Gid { get; set; }

		[Required]
		[JsonRequired, JsonPropertyName("username")]
		[LdapAttribute("cn")]
		public virtual string Username { get; set; } = string.Empty;

		[Required, MinLength(3, ErrorMessage = "The field First name must be a string with a minimum length of '3'.")]
		[JsonRequired, JsonPropertyName("first_name")]
		[LdapAttribute("givenName")]
		public virtual string FirstName { get; set; } = string.Empty;

		[Required, MinLength(3, ErrorMessage = "The field Last name must be a string with a minimum length of '3'.")]
		[JsonRequired, JsonPropertyName("last_name")]
		[LdapAttribute("sn")]
		public virtual string LastName { get; set; } = string.Empty;

		[AllowNull]
		[JsonPropertyName("middle_name")]
		public virtual string? MiddleName { get; set; }

		[Required, EmailAddress]
		[JsonRequired, JsonPropertyName("email")]
		[LdapAttribute("mail")]
		public virtual string Email { get; set; } = string.Empty;

		[Required]
		[JsonRequired, JsonPropertyName("home_directory")]
		[LdapAttribute("homeDirectory")]
		public virtual string HomeDirectory { get; set; } = string.Empty;

		[Required, PasswordPropertyText, MinLength(8)]
		[JsonRequired, JsonPropertyName("password")]
		[LdapAttribute("userPassword")]
		public virtual string Password { get; set; } = string.Empty;

		[JsonPropertyName("full_name")]
		[LdapAttribute("displayName")]
		public virtual string FullName { get; set; } = string.Empty;

		public string GetFullName() => string.Join(" ", FirstName, MiddleName, LastName);
	}
}

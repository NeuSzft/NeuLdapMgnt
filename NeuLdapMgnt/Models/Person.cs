using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;

namespace NeuLdapMgnt.Models
{
	[LdapObjectClasses("inetOrgPerson", "posixAccount")]
	public abstract class Person
	{
		private string lastName = string.Empty;
		private string firstName = string.Empty;
		private string? middleName;

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
		public virtual string FirstName
		{
			get => firstName;
			set
			{
				firstName = value;
				FullName = GetFullName();
			}
		}

		[Required, MinLength(3, ErrorMessage = "The field Last name must be a string with a minimum length of '3'.")]
		[JsonRequired, JsonPropertyName("last_name")]
		[LdapAttribute("sn")]
		public virtual string LastName
		{
			get => lastName;
			set
			{
				lastName = value;
				FullName = GetFullName();
			}
		}

		[AllowNull]
		[JsonPropertyName("middle_name")]
		public virtual string? MiddleName { get => middleName; set
			{
				middleName = value;
				FullName = GetFullName();
			}
		}

		[Required, EmailAddress]
		[JsonRequired, JsonPropertyName("email")]
		[LdapAttribute("mail")]
		public virtual string Email { get; set; } = string.Empty;

		[Required]
		[JsonRequired, JsonPropertyName("home_directory")]
		[LdapAttribute("homeDirectory")]
		public virtual string HomeDirectory { get; set; } = string.Empty;

		[Required, PasswordPropertyText, MinLength(8)]
		[JsonInclude, JsonPropertyName("password")]
		[LdapAttribute("userPassword", false)]
		public virtual string Password { get; set; } = string.Empty;

		[JsonPropertyName("full_name")]
		[LdapAttribute("displayName")]
		public virtual string FullName { get; set; } = string.Empty;

		private string GetFullName()
		{
			TextInfo textInfo = new CultureInfo("hu-HU", false).TextInfo;
			string capitalizedFirstName = textInfo.ToTitleCase(FirstName);
			string? capitalizedMiddleName = string.IsNullOrEmpty(MiddleName) ? null : textInfo.ToTitleCase(MiddleName);
			string capitalizedLastName = textInfo.ToTitleCase(LastName);

			return string.Join(' ', new[] { capitalizedFirstName, capitalizedMiddleName, capitalizedLastName }.Where(x => x is not null));
		}

		public string GetUsername()
		{
			return $"{FirstName.PadRight(3, '_')[..3]}{LastName.PadRight(3, '_')[..3]}".ToLower();
		}
	}
}

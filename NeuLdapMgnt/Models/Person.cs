using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using NeuLdapMgnt.Models.CustomValidationAttributes;

namespace NeuLdapMgnt.Models
{
	[LdapObjectClasses("inetOrgPerson", "posixAccount")]
	public abstract class Person
	{
		private string lastName = string.Empty;
		private string firstName = string.Empty;
		private string middleName = string.Empty;
		private string homeDirectory = string.Empty;

		[Required(ErrorMessage = "User ID is required.")]
		[JsonRequired, JsonPropertyName("uid")]
		[LdapAttribute("uidNumber")]
		public virtual int Uid { get; set; }

		[Required(ErrorMessage = "Group ID is required.")]
		[JsonRequired, JsonPropertyName("gid")]
		[LdapAttribute("gidNumber")]
		public virtual int Gid { get; set; }

		[Required(ErrorMessage = "Username is required.")]
		[JsonRequired, JsonPropertyName("username")]
		[LdapAttribute("cn")]
		public virtual string Username { get; set; } = string.Empty;

		[Required(ErrorMessage = "First name is required.")]
		[FirstName]
		[JsonRequired, JsonPropertyName("first_name")]
		[LdapAttribute("givenName")]
		public virtual string FirstName
		{
			get => firstName;
			set
			{
				firstName = value.Trim();
				FullName = GetFullName();
				Username = GetUsername();
				HomeDirectory = GetHomeDirectory();
			}
		}

		[Required(ErrorMessage = "Last name is required.")]
		[LastName]
		[JsonRequired, JsonPropertyName("last_name")]
		[LdapAttribute("sn")]
		public virtual string LastName
		{
			get => lastName;
			set
			{
				lastName = value.Trim();
				FullName = GetFullName();
				Username = GetUsername();
				HomeDirectory = GetHomeDirectory();
			}
		}

		[AllowNull]
		[MiddleName]
		[JsonPropertyName("middle_name")]
		public virtual string MiddleName
		{
			get => middleName;
			set
			{
				middleName = value is null ? string.Empty : value.Trim();
				FullName = GetFullName();
			}
		}

		[Email]
		[JsonRequired, JsonPropertyName("email")]
		[LdapAttribute("mail")]
		public virtual string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Directory is required.")]
		[Directory]
		[JsonRequired, JsonPropertyName("home_directory")]
		[LdapAttribute("homeDirectory")]
		public virtual string HomeDirectory
		{
			get => homeDirectory;
			set
			{
				if (string.IsNullOrWhiteSpace(value) || string.IsNullOrEmpty(value))
				{
					homeDirectory = "/home/";
					return;
				}
				homeDirectory = value.Replace(" ", "");
			}
		}
		[Required(ErrorMessage = "Password is required.")]
		[Password]
		[PasswordPropertyText]
		[JsonInclude, JsonPropertyName("password")]
		[LdapAttribute("userPassword", false)]
		public virtual string Password { get; set; } = string.Empty;

		[JsonPropertyName("full_name")]
		[LdapAttribute("displayName")]
		public virtual string FullName { get; set; } = string.Empty;

		private string GetFullName()
		{
			StringBuilder builder = new();
			TextInfo textInfo = new CultureInfo("hu-HU", false).TextInfo;
			string capitalizedFirstName = textInfo.ToTitleCase(FirstName);
			string capitalizedMiddleName = string.IsNullOrEmpty(MiddleName) ? string.Empty : textInfo.ToTitleCase(MiddleName);
			string capitalizedLastName = textInfo.ToTitleCase(LastName);

			if (string.IsNullOrEmpty(capitalizedMiddleName))
			{
				builder.Append(capitalizedFirstName + ' ');
				builder.Append(capitalizedLastName);
			}
			else
			{
				builder.Append(capitalizedFirstName + ' ');
				builder.Append(capitalizedMiddleName + ' ');
				builder.Append(capitalizedLastName);
			}

			return builder.ToString().Trim();
		}

		public string GetUsername()
		{
			return $"{LastName.PadRight(3, '_')[..3]}{FirstName.PadRight(3, '_')[..3]}".ToLower();
		}

		private string GetHomeDirectory()
		{
			return string.IsNullOrEmpty(Username) ? "/home/" : $"/home/{GetUsername()}";
		}
	}
}

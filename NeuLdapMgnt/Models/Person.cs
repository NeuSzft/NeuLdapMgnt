using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Serialization;
using NeuLdapMgnt.Models.CustomValidationAttributes;
using NeuLdapMgnt.Models.CustomValidationAttributes.NameAttributes;

namespace NeuLdapMgnt.Models
{
	[LdapObjectClasses("inetOrgPerson", "posixAccount")]
	public abstract class Person
	{
		private string lastName = string.Empty;
		private string firstName = string.Empty;
		private string middleName = string.Empty;
		private string classYear = string.Empty;
		private string classGroup = string.Empty;
		private string @class = string.Empty;
		private string homeDirectory = string.Empty;

		public static readonly string[] ClassYears = { "9", "10", "11", "12", "13", "1/13", "2/14" };
		public static readonly string[] ClassGroups = { "A", "B", "C", "D", "E", "Ny", "A.RSZE", "B.RSZE" };

		[Required(ErrorMessage = "User ID is required.")]
		[JsonRequired, JsonPropertyName("uid")]
		[LdapAttribute("uidNumber")]
		public abstract int Uid { get; set; }

		[Required(ErrorMessage = "Group ID is required.")]
		[JsonRequired, JsonPropertyName("gid")]
		[LdapAttribute("gidNumber")]
		public abstract int Gid { get; set; }

		[Required(ErrorMessage = "Username is required.")]
		[JsonRequired, JsonPropertyName("username")]
		[LdapAttribute("cn")]
		public string Username { get; set; } = string.Empty;

		[Required(ErrorMessage = "First name is required.")]
		[FirstName]
		[JsonRequired, JsonPropertyName("first_name")]
		[LdapAttribute("givenName")]
		public string FirstName
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
		public string LastName
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
		public string MiddleName
		{
			get => middleName;
			set
			{
				middleName = value is null ? string.Empty : value.Trim();
				FullName = GetFullName();
			}
		}

		[Required(ErrorMessage = "Class is required.")]
		[Class]
		[JsonRequired, JsonPropertyName("class")]
		[LdapAttribute("roomNumber")]
		public string Class
		{
			get => @class;
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					@class = string.Empty;
					return;
				}

				@class = value;
				var classSplit = @class.Split('.');
				classYear = classSplit[0];
				classGroup = classSplit[1];
			}
		}

		[JsonIgnore]
		public string ClassYear
		{
			get => classYear;
			set
			{
				if (string.IsNullOrEmpty(value)) return;
				classYear = value;
				UpdateClass();
			}
		}

		[JsonIgnore]
		public string ClassGroup
		{
			get => classGroup;
			set
			{
				if (string.IsNullOrEmpty(value)) return;
				classGroup = value;
				UpdateClass();
			}
		}

		[Email]
		[JsonRequired, JsonPropertyName("email")]
		[LdapAttribute("mail")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Directory is required.")]
		[Directory]
		[JsonRequired, JsonPropertyName("home_directory")]
		[LdapAttribute("homeDirectory")]
		public string HomeDirectory
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

		[Password]
		[PasswordPropertyText]
		[JsonInclude, JsonPropertyName("password")]
		[LdapAttribute("userPassword", true)]
		public virtual string? Password { get; set; } = string.Empty;

		[JsonPropertyName("full_name")]
		[LdapAttribute("displayName")]
		public string FullName { get; set; } = string.Empty;

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
			return $"{FirstName.PadRight(3, '_')[..3]}{LastName.PadRight(3, '_')[..3]}".ToLower();
		}

		public int ClassYearOrderValue()
		{
			if (ClassGroup == "Ny")
			{
				return 0;
			}

			if (ClassYear.Contains('/'))
			{
				return int.Parse(ClassYear[2..] + '0');
			}

			return int.Parse(ClassYear);
		}
		private void UpdateClass()
		{
			@class = ClassYear.Contains('/') ? $"{ClassYear}{ClassGroup}" : $"{ClassYear}.{ClassGroup}";
		}

		private string GetHomeDirectory()
		{
			return string.IsNullOrEmpty(Username) ? "/home/" : $"/home/{GetUsername()}";
		}
	}
}

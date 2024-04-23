using System;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using NeuLdapMgnt.Models.CustomValidationAttributes.IdAttributes;
using NeuLdapMgnt.Models.CustomValidationAttributes;

namespace NeuLdapMgnt.Models
{
	public sealed class Student : Person, IEquatable<Student>
	{
		public const long IdMinValue = 70000000000;
		public const long IdMaxValue = 79999999999;
		public const int UidMinValue = 6000;
		public const int UidMaxValue = 9999;
		public const int GidMinValue = 6000;
		public const int GidMaxValue = 9999;

		[Required(ErrorMessage = "OM is required.")]
		[IdStudent(IdMinValue, IdMaxValue)]
		[JsonPropertyName("id")]
		[LdapAttribute("uid")]
		public long Id { get; set; } = IdMinValue;

		[Required]
		[UserId(UidMinValue, UidMaxValue)]
		[JsonRequired, JsonPropertyName("uid")]
		[LdapAttribute("uidNumber")]
		public override int Uid { get; set; } = UidMinValue;

		[Required]
		[GroupId(GidMinValue, GidMaxValue)]
		[JsonRequired, JsonPropertyName("gid")]
		[LdapAttribute("gidNumber")]
		public override int Gid { get; set; } = GidMinValue;

		[Required(ErrorMessage = "Class is required for students.")]
		[Class]
		[JsonRequired, JsonPropertyName("class")]
		[LdapAttribute("roomNumber")]
		public string Class { get; set; } = string.Empty;

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

		public override bool Equals(object? obj)
		{
			return Equals(obj as Student);
		}

		public override int GetHashCode()
		{
			HashCode hash = new();
			hash.Add(Id);
			hash.Add(Uid);
			hash.Add(Gid);
			hash.Add(Username);
			hash.Add(FirstName);
			hash.Add(MiddleName);
			hash.Add(LastName);
			hash.Add(Class);
			hash.Add(HomeDirectory);
			hash.Add(Email);
			hash.Add(Password);
			return hash.ToHashCode();
		}
	}
}

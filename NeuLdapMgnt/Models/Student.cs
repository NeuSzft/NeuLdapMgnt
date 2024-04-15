using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NeuLdapMgnt.Models
{
	public sealed class Student : Person, IEquatable<Student>
	{
		public static readonly (long Min, long Max) AllowedIdRange = new(70000000000, 79999999999);
		public static readonly (int Min, int Max) AllowedUidRange = new(6000, 6999);
		public static readonly (int Min, int Max) AllowedGidRange = new(6000, 6999);
		public static readonly string[] AllowedYears = { "9", "10", "11", "12", "13", "1/13", "2/14" };
		public static readonly string[] AllowedGroups = { "A", "B", "C", "D", "E", "Ny", "A.RSZE", "B.RSZE" };

		private string classYear = string.Empty;
		private string classGroup = string.Empty;
		private string @class = string.Empty;

		[Required(ErrorMessage = "OM is required.")]
		[Range(70000000000, 79999999999)]
		[JsonPropertyName("id")]
		[LdapAttribute("uid")]
		public long Id { get; set; } = AllowedIdRange.Min;

		[Required]
		[Range(6000, 6999)]
		[JsonRequired, JsonPropertyName("uid")]
		[LdapAttribute("uidNumber")]
		public override int Uid { get; set; } = AllowedUidRange.Min;

		[Required, Range(6000, 6999)]
		[JsonRequired, JsonPropertyName("gid")]
		[LdapAttribute("gidNumber")]
		public override int Gid { get; set; } = AllowedGidRange.Min;

		[Required, RegularExpression(@"^((9\.Ny)|((9|10|11|12|13)\.[A-E])|((1\/13|2\/14)[A-B](\.RSZE)?))$", ErrorMessage = "Class does not exist")]
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

		private void UpdateClass() => @class = ClassYear.Contains('/') ? $"{ClassYear}{ClassGroup}" : $"{ClassYear}.{ClassGroup}";

		public int ClassYearOrderValue()
		{
			if (ClassGroup == "Ny")
				return 0;
			if (ClassYear.Contains('/'))
				return int.Parse(ClassYear[2..] + '0');
			return int.Parse(ClassYear);
		}

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

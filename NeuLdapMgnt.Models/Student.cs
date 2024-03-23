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
		public static readonly (int Min, int Max) AllowedYearsRange = new(8, 14);
		public static readonly (int Min, int Max) AllowedSubGroupsRange = new(1, 2);
		public static readonly string[] AllowedGroups = new[] { "A", "B", "C", "D", "E", "Ny", "Rsze", "Szft" };

		private int classYear;
		private string classGroup = string.Empty;
		private int? classSubGroup = null;

		[Required, Range(70000000000, 79999999999)]
		public override long Id { get; set; } = AllowedIdRange.Min;

		[Required, Range(6000, 6999)]
		public override int Uid { get; set; } = AllowedUidRange.Min;

		[Required, Range(6000, 6999)]
		public override int Gid { get; set; } = AllowedGidRange.Min;

		[Required, RegularExpression(@"^((9|10|11|12)\.[A-E])|([1-2]/(13|14)\.[A-B](\.Ny|\.Rsze|\.Szft)?)$", ErrorMessage = "Valid class formats: '9.A' '12.E' '2/14A'")]
		[JsonRequired, JsonPropertyName("class")]
		[LdapAttribute("roomNumber")]
		public string Class { get; set; } = string.Empty;

		public int ClassYear
		{
			get => classYear;
			set
			{
				classYear = value;
				UpdateClass();
			}
		}

		public string ClassGroup
		{
			get => classGroup;
			set
			{
				classGroup = value;
				UpdateClass();
			}
		}

		public int? ClassSubGroup
		{
			get => classSubGroup;
			set
			{
				classSubGroup = value;
				UpdateClass();
			}
		}

		private void UpdateClass()
		{
			Class = $"{(ClassSubGroup.HasValue ? $"{ClassSubGroup}/" : "")}{ClassYear}.{ClassGroup}";
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

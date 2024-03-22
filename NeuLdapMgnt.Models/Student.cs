using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace NeuLdapMgnt.Models
{
	public sealed class Student : Person, IEquatable<Student>
	{
		public static readonly (long Min, long Max) AllowedIdRange = new(70000000000, 79999999999);
		public static readonly (int Min, int Max) AllowedUidRange = new(6000, 6999);
		public static readonly (int Min, int Max) AllowedGidRange = new(6000, 6999);
		public static readonly int[] AllowedYears = new[] { 8, 9, 10, 11, 12, 13, 14 };
		public static readonly string[] AllowedGroups = new[] { "A", "B", "C", "D", "E", "Ny", "Rsze", "Szft" };
		public static readonly int[] AllowedSubGroups = new[] { 1, 2 };

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

		public int ClassYear { get; set; } = 0;

		public string ClassGroup { get; set; } = string.Empty;

		public int? ClassSubGroup { get; set; } = null;

		public string GetClass()
		{
			return $"{(ClassSubGroup is null ? "" : $"{ClassSubGroup}/")}" +
				$"{ClassYear}." +
				$"{ClassGroup[0].ToString().ToUpper()}{ClassGroup[1..]}";
		}

		public bool Equals(Student? other)
		{
			if (other == null) return false;

			return Id == other.Id
				&& FirstName == other.FirstName
				&& MiddleName == other.MiddleName
				&& LastName == other.LastName
				&& GetClass() == other.GetClass()
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

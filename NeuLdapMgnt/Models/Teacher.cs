using System;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using NeuLdapMgnt.Models.CustomValidationAttributes.IdAttributes;

namespace NeuLdapMgnt.Models;

public sealed class Teacher : Person, IEquatable<Teacher>
{
	private string _id = string.Empty;

	public const int UidMinValue = 4000;
	public const int UidMaxValue = 5999;
	public const int GidMinValue = 4000;
	public const int GidMaxValue = 5999;

	[Required]
	[IdTeacher]
	[JsonPropertyName("id")]
	[LdapAttribute("uid")]
	public string Id
	{
		get => _id;
		set => _id = RemoveDiacritics(value).ToLower();
	}

	[Required]
	[UserId(UidMinValue, UidMaxValue)]
	[JsonRequired, JsonPropertyName("uid")]
	[LdapAttribute("uidNumber")]
	public override int Uid { get; set; } = UidMinValue;

	[Required]
	[UserId(GidMinValue, GidMaxValue)]
	[JsonRequired, JsonPropertyName("gid")]
	[LdapAttribute("gidNumber")]
	public override int Gid { get; set; } = GidMinValue;

	[JsonRequired, JsonPropertyName("class")]
	[LdapAttribute("roomNumber")]
	public string Class { get; set; } = "-";

	[JsonIgnore]
	public bool IsAdmin { get; set; }
	
	public bool Equals(Teacher? other)
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
		return Equals(obj as Teacher);
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

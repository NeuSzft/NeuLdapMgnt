using NeuLdapMgnt.Models.CustomValidationAttributes.IdAttributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NeuLdapMgnt.Models;

public sealed class Employee : Person, IEquatable<Employee>
{
    private string _id = string.Empty;

    public const int UidMinValue = 4000;
    public const int UidMaxValue = 5999;
    public const int GidMinValue = 4000;
    public const int GidMaxValue = 5999;

    [Required]
    [IdEmployee]
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

    [JsonInclude, JsonPropertyName("is_admin")]
    [LdapFlag("admin")]
    public bool IsAdmin { get; set; }

    [JsonInclude, JsonPropertyName("is_teacher")]
    [LdapFlag("teacher")]
    public bool IsTeacher { get; set; }

    public bool Equals(Employee? other)
    {
        if (other == null) return false;

        return Id == other.Id
            && GivenName == other.GivenName
            && MiddleName == other.MiddleName
            && Surname == other.Surname
            && Class == other.Class
            && Username == other.Username
            && Uid == other.Uid
            && Gid == other.Gid
            && Email == other.Email
            && HomeDirectory == other.HomeDirectory
            && Password == other.Password;
    }

    public override bool Equals(object? obj) => Equals(obj as Employee);

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(Id);
        hash.Add(Uid);
        hash.Add(Gid);
        hash.Add(Username);
        hash.Add(GivenName);
        hash.Add(MiddleName);
        hash.Add(Surname);
        hash.Add(Class);
        hash.Add(HomeDirectory);
        hash.Add(Email);
        hash.Add(Password);
        return hash.ToHashCode();
    }
}

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using NeuLdapMgnt.Models.CustomValidationAttributes.TeacherAttributes;

namespace NeuLdapMgnt.Models;

public class Teacher : Person
{
	public const int UidMinValue = 6999;
	public const int UidMaxValue = 6000;
	public const int GidMinValue = 6999;
	public const int GidMaxValue = 6000;

	[Required]
	[TeacherId]
	[JsonPropertyName("id")]
	[LdapAttribute("uid")]
	public string Id { get; set; } = string.Empty;

	[Required]
	[TeacherUserId(UidMinValue, UidMaxValue)]
	[JsonRequired, JsonPropertyName("uid")]
	[LdapAttribute("uidNumber")]
	public override int Uid { get; set; } = UidMinValue;

	[Required]
	[TeacherGroupId(GidMinValue, GidMaxValue)]
	[JsonRequired, JsonPropertyName("gid")]
	[LdapAttribute("gidNumber")]
	public override int Gid { get; set; } = GidMinValue;
}

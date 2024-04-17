using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using NeuLdapMgnt.Models.CustomValidationAttributes.IdAttributes;

namespace NeuLdapMgnt.Models;

public class Admin : Person
{
	public const int UidMinValue = 1000;
	public const int UidMaxValue = 1999;
	public const int GidMinValue = 1000;
	public const int GidMaxValue = 1999;

	[Required]
	[IdTeacher]
	[JsonPropertyName("id")]
	[LdapAttribute("uid")]
	public string Id { get; set; } = string.Empty;

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
}

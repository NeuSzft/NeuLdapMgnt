using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using NeuLdapMgnt.Models.CustomValidationAttributes.TeacherAttributes;

namespace NeuLdapMgnt.Models;

public class Teacher : Person
{
	[Required]
	[TeacherId]
	[JsonPropertyName("id")]
	[LdapAttribute("uid")]
	public string Id { get; set; } = string.Empty;

	[Required]
	[TeacherUserId]
	[JsonRequired, JsonPropertyName("uid")]
	[LdapAttribute("uidNumber")]
	public override int Uid { get; set; } = 4000;

	[Required]
	[TeacherGroupId]
	[JsonRequired, JsonPropertyName("gid")]
	[LdapAttribute("gidNumber")]
	public override int Gid { get; set; } = 4000;
}

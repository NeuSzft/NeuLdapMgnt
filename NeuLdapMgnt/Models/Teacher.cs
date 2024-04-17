﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using NeuLdapMgnt.Models.CustomValidationAttributes.IdAttributes;

namespace NeuLdapMgnt.Models;

public class Teacher : Person
{
	public const int UidMinValue = 4000;
	public const int UidMaxValue = 5999;
	public const int GidMinValue = 4000;
	public const int GidMaxValue = 5999;

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

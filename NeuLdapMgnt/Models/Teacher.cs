using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NeuLdapMgnt.Models;

public class Teacher : Person {
    [RegularExpression(@"^.+\..+$")]
    [JsonPropertyName("id")]
    [LdapAttribute("uid")]
    public string Id { get; set; }

    [Required, Range(4001, 5999)]
    [JsonRequired, JsonPropertyName("uid")]
    [LdapAttribute("uidNumber")]
    public override int Uid { get; set; } = 4001;

    [Required, Range(4001, 5999)]
    [JsonRequired, JsonPropertyName("gid")]
    [LdapAttribute("gidNumber")]
    public override int Gid { get; set; } = 4001;
}

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NeuLdapMgnt.Models;

// TODO: Add xml doc comments to this class
public record LdapDbDump {
    [JsonRequired, JsonPropertyName("students")]
    public required IEnumerable<Student> Students { get; init; }

    [JsonRequired, JsonPropertyName("teachers")]
    public required IEnumerable<Teacher> Teachers { get; init; }

    [JsonRequired, JsonPropertyName("inactives")]
    public required IEnumerable<string> Inactives { get; init; }

    [JsonRequired, JsonPropertyName("admins")]
    public required IEnumerable<string> Admins { get; init; }

    [JsonRequired, JsonPropertyName("values")]
    public required Dictionary<string, string> Values { get; init; }
}

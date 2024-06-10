using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NeuLdapMgnt.Models;

/// <summary>Represents the contents of an LDAP database.</summary>
public record LdapDbDump {
    /// <summary>The <see cref="Student"/> entities.</summary>
    [JsonRequired, JsonPropertyName("students")]
    public required IEnumerable<Student> Students { get; init; }

    /// <summary>The <see cref="Employee"/> entities.</summary>
    [JsonRequired, JsonPropertyName("teachers")]
    public required IEnumerable<Employee> Teachers { get; init; }

    /// <summary>The uids of the "<c>inactive</c>" group.</summary>
    [JsonRequired, JsonPropertyName("inactives")]
    public required IEnumerable<string> Inactives { get; init; }

    /// <summary>The uids of the "<c>admin</c>" group.</summary>
    [JsonRequired, JsonPropertyName("admins")]
    public required IEnumerable<string> Admins { get; init; }

    /// <summary>A dictionary of the key-value pairs.</summary>
    [JsonRequired, JsonPropertyName("values")]
    public required Dictionary<string, string> Values { get; init; }
}

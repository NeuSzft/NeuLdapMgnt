using System;
using System.Collections.Immutable;

namespace NeuLdapMgnt.Models;

/// <summary>Specifies which LDAP object classes does the entry have.</summary>
/// <example><code>
/// [LdapObjectClasses("inetOrgPerson", "posixAccount")]
/// public class Example {
///     ...
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Class)]
public sealed class LdapObjectClassesAttribute : Attribute {
    /// <summary>Names of the LDAP object classes.</summary>
    public readonly ImmutableArray<string> Classes;

    /// <summary>Initializes a new instance of the <see cref="LdapObjectClassesAttribute"/> class.</summary>
    /// <param name="classes">Names of the LDAP object classes.</param>
    public LdapObjectClassesAttribute(params string[] classes) => Classes = classes.ToImmutableArray();
}

/// <summary>Specifies the name of the LDAP attribute.</summary>
/// <example><code>
/// public class Example {
///     [LdapAttribute("uid")]
///     public long Id { get; set; }
///
///     [LdapAttribute("userPassword", false)]
///     public string Password { get; set; }
///     ...
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Property)]
public sealed class LdapAttributeAttribute : Attribute {
    /// <summary>Name of the LDAP attribute.</summary>
    public readonly string Name;

    /// <summary>Determines whether the attribute should be returned from the database.</summary>
    public readonly bool ReturnFormDb;

    /// <summary>Initializes a new instance of the <see cref="LdapObjectClassesAttribute"/> class.</summary>
    /// <param name="name">Name of the LDAP attribute.</param>
    /// <param name="returnFromDb">Determines whether the attribute should be returned from the database.</param>
    public LdapAttributeAttribute(string name, bool returnFromDb = true) {
        Name         = name;
        ReturnFormDb = returnFromDb;
    }
}

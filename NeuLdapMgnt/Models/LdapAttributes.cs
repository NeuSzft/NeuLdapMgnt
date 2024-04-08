using System;

namespace NeuLdapMgnt.Models;

/// <summary>Specifies which LDAP object classes does the entry have.</summary>
/// <param name="classes">Names of the LDAP object classes.</param>
/// <example><code>
/// [LdapObjectClasses("inetOrgPerson", "posixAccount")]
/// public class Example {
///     ...
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Class)]
public sealed class LdapObjectClassesAttribute : Attribute {
    /// <summary>Names of the LDAP object classes.</summary>
    public readonly string[] Classes;

    /// <summary>Initializes a new instance of the <see cref="LdapObjectClassesAttribute"/> class.</summary>
    public LdapObjectClassesAttribute(params string[] classes) => Classes = classes;
}

/// <summary>Specifies the name of the LDAP attribute.</summary>
/// <param name="name">Name of the LDAP attribute.</param>
/// <example><code>
/// public class Example {
///     [LdapAttribute("uid")]
///     public long Id { get; set; }
///
///     [LdapAttribute("cn")]
///     public string Username { get; set; }
///     ...
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Property)]
public sealed class LdapAttributeAttribute : Attribute {
    /// <summary>Name of the LDAP attribute.</summary>
    public readonly string Name;

    /// <summary>Initializes a new instance of the <see cref="LdapObjectClassesAttribute"/> class.</summary>
    public LdapAttributeAttribute(string name) => Name = name;
}

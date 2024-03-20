using System;

namespace NeuLdapMgnt.Models;

[AttributeUsage(AttributeTargets.Property)]
public class LdapAttributeAttribute : Attribute {
    public readonly string Name;

    public LdapAttributeAttribute(string name) => Name = name;
}

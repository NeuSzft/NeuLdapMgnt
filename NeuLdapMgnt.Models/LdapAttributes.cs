using System;

namespace NeuLdapMgnt.Models;

[AttributeUsage(AttributeTargets.Class)]
public class LdapObjectClassesAttribute : Attribute {
    public readonly string[] Classes;

    public LdapObjectClassesAttribute(params string[] classes) => Classes = classes;
}

[AttributeUsage(AttributeTargets.Property)]
public class LdapAttributeAttribute : Attribute {
    public readonly string Name;

    public LdapAttributeAttribute(string name) => Name = name;
}

using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Reflection;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Connectors;

public static class StudentConnector {
    public static bool StudentExists(this LdapHelper helper, string id) {
        SearchRequest   request  = new($"uid={id},ou=students,{helper.DnBase}", LdapHelper.AnyFilter, SearchScope.Base, null);
        SearchResponse? response = helper.TryRequest(request) as SearchResponse;

        return response?.Entries.Count == 1;
    }

    public static IEnumerable<Student> GetAllStudents(this LdapHelper helper) {
        SearchRequest   request  = new($"ou=students,{helper.DnBase}", LdapHelper.AnyFilter, SearchScope.Subtree, null);
        SearchResponse? response = helper.TryRequest(request) as SearchResponse;

        if (response is null)
            yield break;

        foreach (SearchResultEntry entry in response.Entries) {
            if (LdapHelper.TryParseEntry<Student>(entry, out _) is { } student)
                yield return student;
        }
    }

    public static Student? TryGetStudent(this LdapHelper helper, string id, out string? error) {
        SearchRequest   request  = new($"uid={id},ou=students,{helper.DnBase}", LdapHelper.AnyFilter, SearchScope.Base, null);
        SearchResponse? response = helper.TryRequest(request, out error) as SearchResponse;

        if (response is null || response.Entries.Count == 0)
            return null;

        return LdapHelper.TryParseEntry<Student>(response.Entries[0], out error);
    }

    public static bool TryAddStudent(this LdapHelper helper, Student student, string id, out string? error) {
        error = null;
        if (helper.StudentExists(id))
            return false;

        AddRequest request = new($"uid={id},ou=students,{helper.DnBase}", new DirectoryAttribute("objectClass", "inetOrgPerson", "posixAccount"));

        Type type = student.GetType();
        foreach (PropertyInfo info in type.GetProperties())
            if (info.GetCustomAttribute(typeof(LdapAttributeAttribute)) is LdapAttributeAttribute attribute)
                request.Attributes.Add(new(attribute.Name, info.GetValue(student)?.ToString()));

        return helper.TryRequest(request, out error) is not null;
    }

    public static bool TryModifyStudent(this LdapHelper helper, Student student, string id, out string? error) {
        error = null;
        if (helper.StudentExists(id))
            return false;

        ModifyRequest request = new($"uid={id},ou=students,{helper.DnBase}");

        Type type = student.GetType();
        foreach (PropertyInfo info in type.GetProperties())
            if (info.GetCustomAttribute(typeof(LdapAttributeAttribute)) is LdapAttributeAttribute attribute) {
                DirectoryAttributeModification mod = new() {
                    Name      = attribute.Name,
                    Operation = DirectoryAttributeOperation.Replace,
                };
                mod.Add(info.GetValue(student)?.ToString());
                request.Modifications.Add(mod);
            }

        return helper.TryRequest(request) is not null;
    }

    public static bool TryDeleteStudent(this LdapHelper helper, string id, out string? error) {
        error = null;
        if (helper.StudentExists(id))
            return false;

        DeleteRequest request = new($"uid={id},ou=students,{helper.DnBase}");

        return helper.TryRequest(request, out error) is not null;
    }
}

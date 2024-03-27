using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.Logging;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api;

public sealed class LdapBindingException : LdapException;

public sealed class LdapHelper(string server, int port, string user, string password) {
    public string   DnBase { get; init; } = string.Empty;
    public ILogger? Logger { get; set; }

    private readonly LdapDirectoryIdentifier _identifier = new(server, port);
    private readonly NetworkCredential       _credential = new(user, password);

    public DirectoryResponse? TryRequest(DirectoryRequest request) => TryRequest(request, out _);

    public DirectoryResponse? TryRequest(DirectoryRequest request, out string? error) {
        using LdapConnection connection = new(_identifier, _credential, AuthType.Basic);
        connection.SessionOptions.ProtocolVersion = 3;

        try {
            connection.Bind();
        }
        catch {
            throw new LdapBindingException();
        }

        try {
            error = null;
            return connection.SendRequest(request);
        }
        catch (DirectoryException e) {
            error = e.GetError();
            Logger?.LogError(e.ToString());
            return null;
        }
    }

    public static readonly string AnyFilter = "(objectClass=*)";

    public static T ParseEntry<T>(SearchResultEntry entry) where T : class, new() {
        T obj = new();

        foreach (PropertyInfo info in typeof(T).GetProperties())
            if (info.GetCustomAttribute<LdapAttributeAttribute>() is { } attribute) {
                string? value = entry.Attributes[attribute.Name].GetValues(typeof(string)).FirstOrDefault() as string;
                info.SetValue(obj, Convert.ChangeType(value, info.PropertyType));
            }

        return obj;
    }

    public static T? TryParseEntry<T>(SearchResultEntry entry, out string? error) where T : class, new() {
        try {
            error = null;
            return ParseEntry<T>(entry);
        }
        catch (Exception e) {
            error = e.GetError();
            return null;
        }
    }

    public static IEnumerable<DirectoryAttribute> GetDirectoryAttribute(object entity) {
        foreach (PropertyInfo info in entity.GetType().GetProperties())
            if (info.GetCustomAttribute(typeof(LdapAttributeAttribute)) is LdapAttributeAttribute attribute)
                yield return new(attribute.Name, info.GetValue(entity)?.ToString() ?? "<!> NULL <!>");
    }
}

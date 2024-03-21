using System;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api;

public sealed class LdapHelper : IDisposable {
    public readonly LdapConnection Connection;

    public string   DnBase { get; init; } = string.Empty;
    public ILogger? Logger { get; set; }

    public LdapHelper(string server, int port, string user, string password) {
        LdapDirectoryIdentifier identifier = new(server, port);
        NetworkCredential       credential = new(user, password);

        Connection = new(identifier, credential, AuthType.Basic) {
            SessionOptions = {
                ProtocolVersion = 3
            }
        };
        Connection.Bind();
    }

    public void Dispose() => Connection.Dispose();

    public DirectoryResponse? TryRequest(DirectoryRequest request) => TryRequest(request, out _);

    public DirectoryResponse? TryRequest(DirectoryRequest request, out string? error) {
        try {
            error = null;
            return Connection.SendRequest(request);
        }
        catch (DirectoryException e) {
            error = e.GetError();
            Logger?.LogError(e.ToString());
            return null;
        }
    }

    public static readonly string AnyFilter = "(objectClass=*)";

    public static T ParseEntry<T>(SearchResultEntry entry) where T : class {
        Type             type = typeof(T);
        ConstructorInfo? ctor = type.GetConstructor(Type.EmptyTypes);
        T                obj  = ctor?.Invoke(null) as T ?? (T)RuntimeHelpers.GetUninitializedObject(type);

        foreach (PropertyInfo info in type.GetProperties())
            if (info.GetCustomAttribute<LdapAttributeAttribute>() is { } attribute) {
                string? value = entry.Attributes[attribute.Name].GetValues(typeof(string)).FirstOrDefault() as string;
                info.SetValue(obj, Convert.ChangeType(value, info.PropertyType));
            }

        return obj;
    }

    public static T? TryParseEntry<T>(SearchResultEntry entry, out string? error) where T : class {
        try {
            error = null;
            return ParseEntry<T>(entry);
        }
        catch (Exception e) {
            error = e.GetError();
            return null;
        }
    }
}

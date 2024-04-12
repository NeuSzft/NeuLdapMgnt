using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.Logging;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api;

/// <summary>The <see cref="LdapBindingException"/> exception is thrown when an <see cref="LdapConnection"/> of <see cref="LdapService"/> fails to bind to the database.</summary>
public sealed class LdapBindingException : LdapException;

/// <summary>Combines a <see cref="DirectoryRequest"/> and a <c>string</c> identifier.</summary>
/// <param name="Request">The <see cref="DirectoryRequest"/>.</param>
/// <param name="Identifier">The identifier of the request.</param>
public record UniqueDirectoryRequest(DirectoryRequest Request, string Identifier);

/// <summary>This class is used to return either a directory response or an error message.</summary>
/// <param name="Response">A <c>nullable</c> <see cref="DirectoryResponse"/> that contains the response on success.</param>
/// <param name="Error">A <c>nullable</c> <c>string</c> that contains the error message on failure.</param>
public record LdapResult(DirectoryResponse? Response, string? Error = null);

/// <summary>The <see cref="LdapService"/> class provides helper methods for easier interaction with a LDAP server.</summary>
public sealed class LdapService {
    /// <summary>The base distinguished name to use for the requests.</summary>
    public string DnBase { get; set; } = string.Empty;

    /// <summary>The logger the helper should use.</summary>
    public ILogger? Logger { get; set; }

    private readonly LdapDirectoryIdentifier _identifier;
    private readonly NetworkCredential       _credential;

    /// <param name="server">The address of the LDAP server.</param>
    /// <param name="port">The port of the LDAP server.</param>
    /// <param name="user">The username to use when connecting to the LDAP server.</param>
    /// <param name="password">The password of the user.</param>
    /// <example><code>var ldapHelper = new LdapHelper("localhost", 389, "cn=admin,dc=example,dc=local", "AdminPass");</code></example>
    public LdapService(string server, int port, string user, string password) {
        _identifier = new(server, port);
        _credential = new(user, password);
    }

    /// <summary>Tries to send a request to the LDAP server.</summary>
    /// <param name="request">The <see cref="DirectoryRequest"/> to be sent.</param>
    /// <returns>A <c>nullable</c> <see cref="DirectoryResponse"/> that either contains the the response to the request or <c>null</c> if there was an error with the request and there was no response.</returns>
    /// <exception cref="LdapBindingException">The connection failed to bind to the database.</exception>
    public DirectoryResponse? TryRequest(DirectoryRequest request) => TryRequest(request, out _);

    /// <summary>Tries to send a request to the LDAP server.</summary>
    /// <param name="request">The <see cref="DirectoryRequest"/> to be sent.</param>
    /// <param name="error">When the method returns, this will contain the error message if there was one. Otherwise it will be set to <c>null</c>.</param>
    /// <returns>A <c>nullable</c> <see cref="DirectoryResponse"/> that either contains the the response to the request or <c>null</c> if there was an error with the request and there was no response.</returns>
    /// <exception cref="LdapBindingException">The connection failed to bind to the database.</exception>
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

    /// <summary>Tries to send multiple requests to the LDAP server.</summary>
    /// <param name="requests">The <see cref="UniqueDirectoryRequest"/>s to send.</param>
    /// <returns>A <see cref="List{T}">List&lt;LdapResult&gt;</see> containing the <see cref="LdapResult"/>s.</returns>
    /// <exception cref="LdapBindingException">The connection failed to bind to the database.</exception>
    public List<LdapResult> TryRequests(IEnumerable<UniqueDirectoryRequest> requests) {
        using LdapConnection connection = new(_identifier, _credential, AuthType.Basic);
        connection.SessionOptions.ProtocolVersion = 3;

        try {
            connection.Bind();
        }
        catch {
            throw new LdapBindingException();
        }

        List<LdapResult> results = new();

        foreach (UniqueDirectoryRequest request in requests)
            try {
                results.Add(new(connection.SendRequest(request.Request)));
            }
            catch (DirectoryException e) {
                string id = request.Identifier;
                Logger?.LogError($"{id}: {e}");
                results.Add(new(null, $"{id}: {e.GetError()}"));
            }

        return results;
    }

    public const string AnyFilter = "(objectClass=*)";

    /// <summary>Creates a new instance of the specified type and tries to set it's properties marked with <see cref="LdapAttributeAttribute"/> based on the <see cref="SearchResultEntry"/>.</summary>
    /// <param name="entry">The <see cref="SearchResultEntry"/> that contains the values that are used to set object's properties.</param>
    /// <param name="getAllAttributes">If <c>true</c> even the attributes that have <c>ReturnFormDb</c> set to <c>false</c> are returned.</param>
    /// <typeparam name="T">The type of the object to be created.</typeparam>
    /// <returns>A new object with the type <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidCastException">The value of the entry cannot be converted into the property's type.</exception>
    /// <exception cref="FormatException">The value of the entry is in an incorrect format and cannot be converted into the property's type.</exception>
    /// <exception cref="OverflowException">Converting the value of the entry into the property's type would result in an overflow.</exception>
    public static T ParseEntry<T>(SearchResultEntry entry, bool getAllAttributes = false) where T : class, new() {
        T obj = new();

        foreach (PropertyInfo info in typeof(T).GetProperties())
            if (info.GetCustomAttribute<LdapAttributeAttribute>() is { } attribute && (attribute.ReturnFormDb || getAllAttributes)) {
                string? value = entry.Attributes[attribute.Name].GetValues(typeof(string)).FirstOrDefault() as string;
                info.SetValue(obj, Convert.ChangeType(value, info.PropertyType));
            }

        return obj;
    }

    /// <summary>Tries to create a new instance of the specified type and set it's properties marked with <see cref="LdapAttributeAttribute"/> based on the <see cref="SearchResultEntry"/>.</summary>
    /// <param name="entry">The <see cref="SearchResultEntry"/> that contains the values that are used to set object's properties.</param>
    /// <param name="error">When the method returns, this will contain the error message if there was one. Otherwise it will be set to <c>null</c>.</param>
    /// <param name="getAllAttributes">If <c>true</c> even the attributes that have <c>ReturnFormDb</c> set to <c>false</c> are returned.</param>
    /// <typeparam name="T">The type of the object to be created.</typeparam>
    /// <returns>A new object with the type <typeparamref name="T"/>.</returns>
    public static T? TryParseEntry<T>(SearchResultEntry entry, out string? error, bool getAllAttributes = false) where T : class, new() {
        try {
            error = null;
            return ParseEntry<T>(entry, getAllAttributes);
        }
        catch (Exception e) {
            error = e.GetError();
            return null;
        }
    }

    /// <summary>Gets all properties of an object that are marked with <see cref="LdapAttributeAttribute"/> as <see cref="DirectoryAttribute"/>s.</summary>
    /// <param name="obj">The object to get the attributes from.</param>
    /// <returns>An <see cref="IEnumerable{T}">IEnumerable&lt;DirectoryAttribute&gt;</see> containing the <see cref="DirectoryAttribute"/>s.</returns>
    /// <remarks>If the property is not set then The <see cref="DirectoryAttribute"/> will contain the value of <c>"&lt;!&gt; NULL &lt;!&gt;"</c></remarks>
    public static IEnumerable<DirectoryAttribute> GetDirectoryAttributes(object obj) {
        foreach (PropertyInfo info in obj.GetType().GetProperties())
            if (info.GetCustomAttribute(typeof(LdapAttributeAttribute)) is LdapAttributeAttribute attribute)
                yield return new(attribute.Name, info.GetValue(obj)?.ToString() ?? "<!> NULL <!>");
    }

    /// <summary>Creates a new <see cref="LdapService"/> using the specified environment variables.</summary>
    /// <param name="serverEnv">The environment variable that specifies the address of the LDAP server.</param>
    /// <param name="portEnv">The environment variable that specifies the port of the LDAP server.</param>
    /// <param name="userEnv">The environment variable that specifies the username to use when connecting to the LDAP server.</param>
    /// <param name="passwordEnv">The environment variable that specifies the password of the user.</param>
    /// <returns>The new <see cref="LdapService"/>.</returns>
    /// <exception cref="ArgumentException">An environment variable is not set or is an empty string.</exception>
    /// <exception cref="FormatException">The value of <paramref name="portEnv"/> cannot be converted into an integer.</exception>
    /// <exception cref="OverflowException">The value of <paramref name="portEnv"/> would overflow an integer.</exception>
    public static LdapService FromEnvs(string serverEnv = "LDAP_ADDRESS", string portEnv = "LDAP_PORT", string userEnv = "LDAP_USERNAME", string passwordEnv = "LDAP_PASSWORD") {
        return new LdapService(
            Environment.GetEnvironmentVariable(serverEnv).ThrowIfNullOrEmpty(serverEnv),
            int.Parse(Environment.GetEnvironmentVariable(portEnv).ThrowIfNullOrEmpty(portEnv)),
            Environment.GetEnvironmentVariable(userEnv).ThrowIfNullOrEmpty(userEnv),
            Environment.GetEnvironmentVariable(passwordEnv).ThrowIfNullOrEmpty(passwordEnv)
        );
    }
}

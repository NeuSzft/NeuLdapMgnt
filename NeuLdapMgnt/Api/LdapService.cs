using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
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
/// <param name="Error">A <c>nullable</c> <see cref="string"/> that contains the error message on failure.</param>
public record LdapResult(DirectoryResponse? Response, string? Error = null);

/// <summary>The <see cref="LdapService"/> class provides helper methods for easier interaction with a LDAP server.</summary>
public sealed class LdapService {
	/// <summary>The domain components to use for the requests.</summary>
	/// <example>"<c>dc=example,dc=com</c>" for example.com</example>
	public string DomainComponents { get; }

	/// <summary>The logger the helper should use.</summary>
	public ILogger? Logger { get; set; }

	private readonly LdapDirectoryIdentifier _identifier;
	private readonly NetworkCredential       _credential;

	/// <param name="server">The address of the LDAP server.</param>
	/// <param name="port">The port of the LDAP server.</param>
	/// <param name="domain">The LDAP domain.</param>
	/// <param name="username">The username to use when connecting to the LDAP server.</param>
	/// <param name="password">The password of the user.</param>
	/// <example><code>var ldapHelper = new LdapHelper("localhost", 389, "example.local", "admin", "admin-password");</code></example>
	public LdapService(string server, int port, string domain, string username, string password) {
		DomainComponents = $"dc={domain.Replace(".", ",dc=")}";
		_identifier      = new LdapDirectoryIdentifier(server, port);
		_credential      = new NetworkCredential($"cn={username},{DomainComponents}", password);
	}

	/// <summary>Tries to send a request to the LDAP server.</summary>
	/// <param name="request">The <see cref="DirectoryRequest"/> to be sent.</param>
	/// <param name="logErrors">Specifies whether the exceptions should be logged. The default is <c>true</c>.</param>
	/// <param name="callerMemberName">Obtains the name of the member that called the method. This parameter should not be explicitly set.</param>
	/// <param name="callerLineNumber">Obtains the line number at which the method was called. This parameter should not be explicitly set.</param>
	/// <param name="callerFilePath">Obtains the path of the file where the method call originates from. This parameter should not be explicitly set.</param>
	/// <returns>A <c>nullable</c> <see cref="DirectoryResponse"/> that either contains the response to the request or <c>null</c> if there was an error with the request and there was no response.</returns>
	/// <exception cref="LdapBindingException">The connection failed to bind to the database.</exception>
	public DirectoryResponse? TryRequest(
		DirectoryRequest          request, bool logErrors = true,
		[CallerMemberName] string callerMemberName = "",
		[CallerLineNumber] int    callerLineNumber = 0,
		[CallerFilePath]   string callerFilePath   = ""
	) => TryRequest(request, out _, logErrors, callerMemberName, callerLineNumber, callerFilePath);

	/// <summary>Tries to send a request to the LDAP server.</summary>
	/// <param name="request">The <see cref="DirectoryRequest"/> to be sent.</param>
	/// <param name="error">When the method returns, this will contain the error message if there was one. Otherwise, it will be set to <c>null</c>.</param>
	/// <param name="logErrors">Specifies whether the exceptions should be logged. The default is <c>true</c>.</param>
	/// <param name="callerMemberName">Obtains the name of the member that called the method. This parameter should not be explicitly set.</param>
	/// <param name="callerLineNumber">Obtains the line number at which the method was called. This parameter should not be explicitly set.</param>
	/// <param name="callerFilePath">Obtains the path of the file where the method call originates from. This parameter should not be explicitly set.</param>
	/// <returns>A <c>nullable</c> <see cref="DirectoryResponse"/> that either contains the response to the request or <c>null</c> if there was an error with the request and there was no response.</returns>
	/// <exception cref="LdapBindingException">The connection failed to bind to the database.</exception>
	public DirectoryResponse? TryRequest(
		DirectoryRequest          request,
		out string?               error,
		bool                      logErrors        = true,
		[CallerMemberName] string callerMemberName = "",
		[CallerLineNumber] int    callerLineNumber = 0,
		[CallerFilePath]   string callerFilePath   = ""
	) {
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
			if (logErrors)
				Logger?.LogError($"{callerMemberName} (line {callerLineNumber}) in {callerFilePath}{Environment.NewLine}- {error}");
			return null;
		}
	}

	/// <summary>Tries to send multiple requests to the LDAP server.</summary>
	/// <param name="requests">The <see cref="UniqueDirectoryRequest"/>s to send.</param>
	/// <param name="logErrors">Specifies whether the exceptions should be logged. The default is <c>true</c>.</param>
	/// <param name="callerMemberName">Obtains the name of the member that called the method. This parameter should not be explicitly set.</param>
	/// <param name="callerLineNumber">Obtains the line number at which the method was called. This parameter should not be explicitly set.</param>
	/// <param name="callerFilePath">Obtains the path of the file where the method call originates from. This parameter should not be explicitly set.</param>
	/// <returns>A <see cref="List{T}">List&lt;LdapResult&gt;</see> containing the <see cref="LdapResult"/>s.</returns>
	/// <exception cref="LdapBindingException">The connection failed to bind to the database.</exception>
	public List<LdapResult> TryRequests(
		IEnumerable<UniqueDirectoryRequest> requests,
		bool                                logErrors        = true,
		[CallerMemberName] string           callerMemberName = "",
		[CallerLineNumber] int              callerLineNumber = 0,
		[CallerFilePath]   string           callerFilePath   = ""
	) {
		using LdapConnection connection = new(_identifier, _credential, AuthType.Basic);
		connection.SessionOptions.ProtocolVersion = 3;

		try {
			connection.Bind();
		}
		catch {
			throw new LdapBindingException();
		}

		List<LdapResult> results = new();

		foreach (var request in requests)
			try {
				results.Add(new LdapResult(connection.SendRequest(request.Request)));
			}
			catch (DirectoryException e) {
				string id    = request.Identifier;
				string error = $"{id}: {e.GetError()}";
				if (logErrors)
					Logger?.LogError($"{callerMemberName} (line {callerLineNumber}) in {callerFilePath}{Environment.NewLine}- {error}");
				results.Add(new LdapResult(null, error));
			}

		return results;
	}

	/// <summary>Recursively deletes all elements of the specified root element.</summary>
	/// <param name="distinguishedName">The distinguished name of the root element.</param>
	/// <returns>A <see cref="List{T}">List&lt;string&gt;</see> containing the errors that occured during the operations.</returns>
	public List<string> EraseTreeElements(string distinguishedName) {
		SearchRequest searchRequest = new(distinguishedName, AnyFilter, SearchScope.OneLevel, null);

		if (TryRequest(searchRequest, out string? error) is not SearchResponse response)
			return [ error! ];

		List<string> errors = new();

		foreach (SearchResultEntry entry in response.Entries) {
			errors.AddRange(EraseTreeElements(entry.DistinguishedName));
			if (TryRequest(new DeleteRequest(entry.DistinguishedName), out error) is null)
				errors.Add(error!);
		}

		return errors;
	}

	/// <summary>An LDAP filter that returns all objects.</summary>
	public const string AnyFilter = "(objectClass=*)";

	/// <summary>Creates a new instance of the specified type and tries to set its properties marked with <see cref="LdapAttributeAttribute"/> based on the <see cref="SearchResultEntry"/>.</summary>
	/// <param name="entry">The <see cref="SearchResultEntry"/> that contains the values that are used to set object's properties.</param>
	/// <param name="getHiddenAttributes">If <c>true</c> even the attributes that are set to be hidden are returned.</param>
	/// <typeparam name="T">The type of the object to be created.</typeparam>
	/// <returns>A new object with the type <typeparamref name="T"/>.</returns>
	/// <exception cref="InvalidCastException">The value of the entry cannot be converted into the property's type.</exception>
	/// <exception cref="FormatException">The value of the entry is in an incorrect format and cannot be converted into the property's type.</exception>
	/// <exception cref="OverflowException">Converting the value of the entry into the property's type would result in an overflow.</exception>
	public static T ParseEntry<T>(SearchResultEntry entry, bool getHiddenAttributes = false) where T : class, new() {
		T obj = new();

		foreach (var info in typeof(T).GetProperties())
			if (
				info.GetCustomAttribute<LdapAttributeAttribute>() is { } attribute
				&& (!attribute.Hidden || getHiddenAttributes)
				&& entry.Attributes.Contains(attribute.Name)
			) {
				string? value = entry.Attributes[attribute.Name].GetValues(typeof(string)).FirstOrDefault() as string;
				info.SetValue(obj, Convert.ChangeType(value, info.PropertyType));
			}
			else if (
				info.GetCustomAttribute<LdapFlagAttribute>() is { } flag
				&& info.PropertyType == typeof(bool)
				&& entry.Attributes.Contains("description")
				&& entry.Attributes["description"].GetValues(typeof(string)).FirstOrDefault() is string flags
			) {
				bool hasFlag = flags.Split('|').Contains(flag.Name);
				info.SetValue(obj, Convert.ChangeType(hasFlag, info.PropertyType));
			}

		return obj;
	}

	/// <summary>Tries to create a new instance of the specified type and set its properties marked with <see cref="LdapAttributeAttribute"/> based on the <see cref="SearchResultEntry"/>.</summary>
	/// <param name="entry">The <see cref="SearchResultEntry"/> that contains the values that are used to set object's properties.</param>
	/// <param name="error">When the method returns, this will contain the error message if there was one. Otherwise, it will be set to <c>null</c>.</param>
	/// <param name="getHiddenAttributes">If <c>true</c> even the attributes that are set to be hidden are returned.</param>
	/// <typeparam name="T">The type of the object to be created.</typeparam>
	/// <returns>A new object with the type <typeparamref name="T"/>.</returns>
	public static T? TryParseEntry<T>(SearchResultEntry entry, out string? error, bool getHiddenAttributes = false) where T : class, new() {
		try {
			error = null;
			return ParseEntry<T>(entry, getHiddenAttributes);
		}
		catch (Exception e) {
			error = e.GetError();
			return null;
		}
	}

	/// <summary>
	/// Gets all properties of an object that are marked with the <see cref="LdapAttributeAttribute"/> as <see cref="DirectoryAttribute"/>s.
	/// Also returns the properties that are marked with the <see cref="LdapFlagAttribute"/> and are <c>true</c>
	/// joined together by <c>|</c> separators as the value of the "description" <see cref="DirectoryAttribute"/>.
	/// </summary>
	/// <param name="obj">The object to get the attributes from.</param>
	/// <param name="getHiddenAttributes">If <c>true</c> even the attributes that are set to be hidden are returned.</param>
	/// <returns>An <see cref="IEnumerable{T}">IEnumerable&lt;DirectoryAttribute&gt;</see> containing the <see cref="DirectoryAttribute"/>s.</returns>
	/// <remarks>If the property is not set then the <see cref="DirectoryAttribute"/> will contain the value of <c>"&lt;!&gt; NULL &lt;!&gt;"</c></remarks>
	public static IEnumerable<DirectoryAttribute> GetDirectoryAttributes(object obj, bool getHiddenAttributes = false) {
		List<string> flags = [ ];

		foreach (var info in obj.GetType().GetProperties())
			if (
				info.GetCustomAttribute<LdapAttributeAttribute>() is { } attribute
				&& (!attribute.Hidden || getHiddenAttributes)
				&& info.GetValue(obj) is { } value
			) {
				yield return new DirectoryAttribute(attribute.Name, value.ToString());
			}
			else if (
				info.GetCustomAttribute<LdapFlagAttribute>() is { } flag
				&& info.GetValue(obj) is true
			) {
				flags.Add(flag.Name);
			}

		if (flags.Count > 0)
			yield return new DirectoryAttribute("description", string.Join('|', flags));
	}

	/// <summary>Creates a new <see cref="LdapService"/> using the specified environment variables.</summary>
	/// <param name="serverEnv">The environment variable that specifies the address of the LDAP server.</param>
	/// <param name="portEnv">The environment variable that specifies the port of the LDAP server.</param>
	/// <param name="domainEnv">The environment variable that specifies the LDAP domain.</param>
	/// <param name="userEnv">The environment variable that specifies the username to use when connecting to the LDAP server.</param>
	/// <param name="passwordEnv">The environment variable that specifies the password of the user.</param>
	/// <returns>The new <see cref="LdapService"/>.</returns>
	/// <exception cref="ArgumentException">An environment variable is not set or is an empty string.</exception>
	/// <exception cref="FormatException">The value of <paramref name="portEnv"/> cannot be converted into an integer.</exception>
	/// <exception cref="OverflowException">The value of <paramref name="portEnv"/> would overflow an integer.</exception>
	public static LdapService FromEnvs(
		string serverEnv   = "LDAP_ADDRESS",
		string portEnv     = "LDAP_PORT",
		string domainEnv   = "LDAP_DOMAIN",
		string userEnv     = "LDAP_USERNAME",
		string passwordEnv = "LDAP_PASSWORD"
	)
		=> new(
			Utils.GetEnv(serverEnv),
			int.Parse(Utils.GetEnv(portEnv)),
			Utils.GetEnv(domainEnv),
			Utils.GetEnv(userEnv),
			Utils.GetEnv(passwordEnv)
		);
}

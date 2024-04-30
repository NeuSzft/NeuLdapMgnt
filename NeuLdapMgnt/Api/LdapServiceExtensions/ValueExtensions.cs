using System.Collections.Generic;
using System.DirectoryServices.Protocols;

namespace NeuLdapMgnt.Api.LdapServiceExtensions;

public static class ValueExtensions {
	/// <summary>Checks if the key-value pair exists within the database.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="key">The key or name of the pair.</param>
	/// <returns><c>true</c> if the key-value pair exists or <c>false</c> if it does not exist or the request fails.</returns>
	public static bool ValueExists(this LdapService ldap, string key) {
		SearchRequest   request  = new($"cn={key},ou=values,{ldap.DomainComponents}", LdapService.AnyFilter, SearchScope.Base, []);
		SearchResponse? response = ldap.TryRequest(request) as SearchResponse;

		return response?.Entries.Count == 1;
	}

	/// <summary>Tries to get the value of a key-value pair from the database.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="key">The key or name of the pair.</param>
	/// <param name="error">When the method returns, this will contain the error message if there was one. Otherwise it will be set to <c>null</c>.</param>
	/// <returns>The value the pair or <c>null</c> if it does not exist or the request fails.</returns>
	public static string? GetValue(this LdapService ldap, string key, out string? error) {
		SearchRequest   request  = new($"cn={key},ou=values,{ldap.DomainComponents}", LdapService.AnyFilter, SearchScope.Base, "description");
		SearchResponse? response = ldap.TryRequest(request, out error) as SearchResponse;

		if (response is null || response.Entries.Count == 0 || response.Entries[0].Attributes["description"].Count == 0)
			return null;

		return response.Entries[0].Attributes["description"][0].ToString();
	}

	/// <summary>Tries to set the value of a key-value pair in the database.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="key">The key or name of the pair.</param>
	/// <param name="value">The new value of the pair.</param>
	/// <param name="error">When the method returns, this will contain the error message if there was one. Otherwise it will be set to <c>null</c>.</param>
	/// <returns><c>true</c> if the key-value pair fas successfully set or <c>false</c> if the request fails.</returns>
	public static bool SetValue(this LdapService ldap, string key, string value, out string? error) {
		ldap.TryRequest(new AddRequest($"ou=values,{ldap.DomainComponents}", "organizationalUnit"), false);

		if (!ldap.ValueExists(key)) {
			AddRequest addRequest = new($"cn={key},ou=values,{ldap.DomainComponents}",
				new("objectClass", "applicationProcess"),
				new("description", value)
			);

			return ldap.TryRequest(addRequest, out error) is not null;
		}

		DirectoryAttributeModification mod = new() {
			Name = "description",
			Operation = DirectoryAttributeOperation.Replace
		};
		mod.Add(value);

		ModifyRequest modRequest = new($"cn={key},ou=values,{ldap.DomainComponents}");
		modRequest.Modifications.Add(mod);

		return ldap.TryRequest(modRequest, out error) is not null;
	}

	/// <summary>Tries to unset (delete) the value of a key-value pair in the database.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="key">The key or name of the pair.</param>
	/// <param name="error">When the method returns, this will contain the error message if there was one. Otherwise it will be set to <c>null</c>.</param>
	/// <param name="successIfNotExists">If <c>true</c> the operation will be treated as a success even if the key-value pair does not exists.</param>
	/// <returns><c>true</c> if the key-value pair fas successfully unset or <c>false</c> if the request fails.</returns>
	public static bool UnsetValue(this LdapService ldap, string key, out string? error, bool successIfNotExists = false) {
		if (ldap.ValueExists(key))
			return ldap.TryRequest(new DeleteRequest($"cn={key},ou=values,{ldap.DomainComponents}"), out error) is not null;

		error = null;
		return successIfNotExists;
	}

	/// <summary>Gets all key-value pair from the database.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="error">When the method returns, this will contain the error message if there was one. Otherwise it will be set to <c>null</c>.</param>
	/// <returns>A <see cref="Dictionary{TKey,TValue}">Dictionary&lt;string, string&gt;</see> containing the key-value pairs or an empty collection if there was an error.</returns>
	public static Dictionary<string, string> GetAllValues(this LdapService ldap, out string? error) {
		SearchRequest   request  = new($"ou=values,{ldap.DomainComponents}", LdapService.AnyFilter, SearchScope.OneLevel, "cn", "description");
		SearchResponse? response = ldap.TryRequest(request, out error) as SearchResponse;

		if (response is null)
			return [];

		Dictionary<string, string> values = new();

		foreach (SearchResultEntry entry in response.Entries)
			values.Add(entry.Attributes["cn"].GetValues(typeof(string))[0].ToString()!, entry.Attributes["description"].GetValues(typeof(string))[0].ToString()!);

		return values;
	}
}

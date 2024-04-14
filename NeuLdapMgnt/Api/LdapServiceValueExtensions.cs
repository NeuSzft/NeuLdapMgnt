using System.Collections.Generic;
using System.DirectoryServices.Protocols;

namespace NeuLdapMgnt.Api;

// TODO: Add xml doc comments to this class
public static class LdapServiceValueExtensions {
    public static bool ValueExists(this LdapService ldap, string name) {
        SearchRequest   request  = new($"cn={name},ou=values,{ldap.DomainComponents}", LdapService.AnyFilter, SearchScope.Base, null);
        SearchResponse? response = ldap.TryRequest(request) as SearchResponse;

        return response?.Entries.Count == 1;
    }

    public static string? GetValue(this LdapService ldap, string name, out string? error) {
        SearchRequest   request  = new($"cn={name},ou=values,{ldap.DomainComponents}", LdapService.AnyFilter, SearchScope.Base, null);
        SearchResponse? response = ldap.TryRequest(request, out error) as SearchResponse;

        if (response is null || response.Entries.Count == 0 || response.Entries[0].Attributes["description"].Count == 0)
            return null;

        return response.Entries[0].Attributes["description"][0].ToString();
    }

    public static bool SetValue(this LdapService ldap, string name, string value, out string? error) {
        ldap.TryRequest(new AddRequest($"ou=values,{ldap.DomainComponents}", "organizationalUnit"));

        if (!ldap.ValueExists(name)) {
            AddRequest addRequest = new($"cn={name},ou=values,{ldap.DomainComponents}",
                new("objectClass", "applicationProcess"),
                new("description", value)
            );

            return ldap.TryRequest(addRequest, out error) is not null;
        }

        DirectoryAttributeModification mod = new() {
            Name      = "description",
            Operation = DirectoryAttributeOperation.Replace
        };
        mod.Add(value);

        ModifyRequest modRequest = new($"cn={name},ou=values,{ldap.DomainComponents}");
        modRequest.Modifications.Add(mod);

        return ldap.TryRequest(modRequest, out error) is not null;
    }

    public static Dictionary<string, string> GetAllValues(this LdapService ldap, out string? error) {
        SearchRequest   request  = new($"ou=values,{ldap.DomainComponents}", LdapService.AnyFilter, SearchScope.OneLevel, null);
        SearchResponse? response = ldap.TryRequest(request, out error) as SearchResponse;

        if (response is null)
            return [];

        Dictionary<string, string> values = new();

        foreach (SearchResultEntry entry in response.Entries)
            values.Add(entry.Attributes["cn"].GetValues(typeof(string))[0].ToString()!, entry.Attributes["description"].GetValues(typeof(string))[0].ToString()!);

        return values;
    }
}

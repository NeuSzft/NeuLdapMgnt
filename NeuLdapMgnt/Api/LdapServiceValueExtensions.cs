using System.DirectoryServices.Protocols;

namespace NeuLdapMgnt.Api;

public static class LdapServiceValueExtensions {
    public static bool ValueExists(this LdapService ldap, string name) {
        SearchRequest   request  = new($"cn={name},ou=values,{ldap.DnBase}", LdapService.AnyFilter, SearchScope.Base, null);
        SearchResponse? response = ldap.TryRequest(request) as SearchResponse;

        return response?.Entries.Count == 1;
    }

    public static string? GetValue(this LdapService ldap, string name, out string? error) {
        SearchRequest   request  = new($"cn={name},ou=values,{ldap.DnBase}", LdapService.AnyFilter, SearchScope.Base, null);
        SearchResponse? response = ldap.TryRequest(request, out error) as SearchResponse;

        if (response is null || response.Entries.Count == 0 || response.Entries[0].Attributes["description"].Count == 0)
            return null;

        return response.Entries[0].Attributes["description"][0].ToString();
    }

    public static bool SetValue(this LdapService ldap, string name, string value, out string? error) {
        ldap.TryRequest(new AddRequest($"ou=values,{ldap.DnBase}", "organizationalUnit"));

        if (!ldap.ValueExists(name)) {
            AddRequest addRequest = new($"cn={name},ou=values,{ldap.DnBase}",
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

        ModifyRequest modRequest = new($"cn={name},ou=values,{ldap.DnBase}");
        modRequest.Modifications.Add(mod);

        return ldap.TryRequest(modRequest, out error) is not null;
    }
}

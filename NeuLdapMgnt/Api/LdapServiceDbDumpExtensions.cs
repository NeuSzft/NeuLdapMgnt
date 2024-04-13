using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api;

// TODO: Add xml doc comments to this class
public static class LdapServiceDbDumpExtensions {
    private static RequestResult EraseDatabase(this LdapService ldap) {
        List<string> errors = ldap.EraseTreeElements(ldap.DnBase);
        return new RequestResult().SetStatus(errors.Count == 0 ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError).SetErrors(errors.ToArray());
    }

    public static RequestResult ImportDatabase(this LdapService ldap, LdapDbDump dump, bool erase = false) {
        if (erase) {
            RequestResult result = ldap.EraseDatabase();
            if (result.Errors.Length > 0)
                return result;
        }

        List<string> errors = new();

        errors.AddRange(ldap.TryAddEntities(dump.Students, student => student.Id.ToString()).Errors);
        errors.AddRange(ldap.TryAddEntities(dump.Teachers, teacher => teacher.Id).Errors);
        errors.AddRange(ldap.TryAddEntitiesToGroup("inactive", dump.Inactives).Errors);
        errors.AddRange(ldap.TryAddEntitiesToGroup("admin", dump.Admins).Errors);

        foreach (var item in dump.Values) {
            ldap.SetValue(item.Key, item.Value, out var error);
            if (error is not null)
                errors.Add(error);
        }

        return new RequestResult().SetStatus(StatusCodes.Status207MultiStatus).SetErrors(errors.ToArray());
    }

    public static RequestResult<LdapDbDump> ExportDatabase(this LdapService ldap) {
        LdapDbDump dump = new() {
            Students  = ldap.GetAllEntities<Student>(),
            Teachers  = ldap.GetAllEntities<Teacher>(),
            Inactives = ldap.GetMembersOfGroup("inactive"),
            Admins    = ldap.GetMembersOfGroup("admin"),
            Values    = ldap.GetAllValues(out var error)
        };

        var response = new RequestResult<LdapDbDump>().SetStatus(StatusCodes.Status207MultiStatus).SetValues(dump);
        if (error is not null)
            response.SetErrors(error);

        return response;
    }
}

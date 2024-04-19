using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api;

public static class LdapServiceDbDumpExtensions {
	/// <summary>Erases the entire LDAP database.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
	private static RequestResult EraseDatabase(this LdapService ldap) {
		List<string> errors = ldap.EraseTreeElements(ldap.DomainComponents);
		return new RequestResult().SetStatus(errors.Count == 0 ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError).SetErrors(errors.ToArray());
	}

	/// <summary>Imports a previous dump into the LDAP database.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="dump">The <see cref="LdapDbDump"/> to import.</param>
	/// <param name="erase">Determines whether the database should be erased before importing.</param>
	/// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
	public static RequestResult ImportDatabase(this LdapService ldap, LdapDbDump dump, bool erase = false) {
		if (erase) {
			RequestResult result = ldap.EraseDatabase();
			if (result.Errors.Length > 0)
				return result;
		}

		List<string> errors = new();

		errors.AddRange(ldap.TryAddEntities(dump.Students, student => student.Id.ToString(), true).Errors);
		errors.AddRange(ldap.TryAddEntities(dump.Teachers, teacher => teacher.Id, true).Errors);
		errors.AddRange(ldap.TryAddEntitiesToGroup("inactive", dump.Inactives).Errors);
		errors.AddRange(ldap.TryAddEntitiesToGroup("admin", dump.Admins).Errors);

		foreach (var item in dump.Values) {
			ldap.SetValue(item.Key, item.Value, out var error);
			if (error is not null)
				errors.Add(error);
		}

		return new RequestResult().SetStatus(StatusCodes.Status207MultiStatus).SetErrors(errors.ToArray());
	}

	/// <summary>Exports the LDAP database into a <see cref="LdapDbDump"/>.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <returns>A <see cref="RequestResult{T}">RequestResult&lt;LdapDbDump&gt;</see> containing the outcome of the operation.</returns>
	public static RequestResult<LdapDbDump> ExportDatabase(this LdapService ldap) {
		LdapDbDump dump = new() {
			Students = ldap.GetAllEntities<Student>(true),
			Teachers = ldap.GetAllEntities<Teacher>(true),
			Inactives = ldap.GetMembersOfGroup("inactive"),
			Admins = ldap.GetMembersOfGroup("admin"),
			Values = ldap.GetAllValues(out var error)
		};

		var response = new RequestResult<LdapDbDump>().SetStatus(StatusCodes.Status207MultiStatus).SetValues(dump);
		if (error is not null)
			response.SetErrors(error);

		return response;
	}
}

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.LdapServiceExtensions;

public static class DbDumpExtensions {
	/// <summary>Erases the entire LDAP database.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
	public static RequestResult EraseDatabase(this LdapService ldap) {
		List<string> errors = ldap.EraseTreeElements(ldap.DomainComponents);
		return new RequestResult().SetStatus(errors.Count == 0 ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError).SetErrors(errors.ToArray());
	}

	/// <summary>Imports a previous dump into the LDAP database.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="dump">The <see cref="LdapDbDump"/> to import.</param>
	/// <param name="overwrite">If <c>true</c> the existing entities and group members will be overwritten as well.</param>
	/// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
	public static RequestResult ImportDatabase(this LdapService ldap, LdapDbDump dump, bool overwrite) {
		List<string> errors = new();

		errors.AddRange(ldap.TryAddEntities(dump.Students, student => student.Id.ToString(), true, overwrite).Errors);
		errors.AddRange(ldap.TryAddEntities(dump.Employees, employee => employee.Id, true, overwrite).Errors);

		foreach (var item in dump.Values) {
			if (!ldap.SetValue(item.Key, item.Value, out string? error))
				errors.Add(error);
		}

		return new RequestResult().SetStatus(StatusCodes.Status207MultiStatus).SetErrors(errors.ToArray());
	}

	/// <summary>Exports the LDAP database into a <see cref="LdapDbDump"/>.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <returns>A <see cref="RequestResult{T}">RequestResult&lt;LdapDbDump&gt;</see> containing the outcome of the operation.</returns>
	public static RequestResult<LdapDbDump> ExportDatabase(this LdapService ldap) {
		var studentResults  = ldap.GetAllEntities<Student>(true);
		var employeeResults = ldap.GetAllEntities<Employee>(true);

		List<string> errors = new();
		errors.AddRange(studentResults.Errors);
		errors.AddRange(employeeResults.Errors);

		LdapDbDump dump = new() {
			Students  = studentResults.Values,
			Employees = employeeResults.Values,
			Values    = ldap.GetAllValues(out string? error)
		};

		var response = new RequestResult<LdapDbDump>().SetStatus(StatusCodes.Status207MultiStatus).SetValues(dump);
		if (error is not null)
			errors.Add(error);

		return response.SetErrors(errors.ToArray());
	}
}

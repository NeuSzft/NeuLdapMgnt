using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Api.LdapServiceExtensions;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class InactiveUserEndpoints {
	public static void MapInactiveUserEndpoints(this WebApplication app) {
		app.MapGet("/api/inactives", (LdapService ldap, HttpRequest request) => {
				IEnumerable<string> employeeUids = ldap.GetAllEntities<Employee>().Values.Where(x => x.IsInactive).Select(x => x.Id);
				IEnumerable<string> studentUids  = ldap.GetAllEntities<Student>().Values.Where(x => x.IsInactive).Select(x => x.Id.ToString());
				string[]            uids         = employeeUids.Concat(studentUids).ToArray();
				return new RequestResult<string>().SetValues(uids).RenewToken(request).ToResult();
			})
			.WithOpenApi()
			.WithTags("Inactive Users")
			.WithDescription("### Returns all UIDs that are part of the \"*inactive*\" group.")
			.RequireAuthorization()
			.Produces<RequestResult<string>>()
			.Produces<string>(StatusCodes.Status401Unauthorized, "text/plain")
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
	}
}

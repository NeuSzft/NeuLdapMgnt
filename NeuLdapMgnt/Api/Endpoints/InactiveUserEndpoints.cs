using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NeuLdapMgnt.Api.LdapServiceExtensions;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.Endpoints;

public static class InactiveUserEndpoints {
	public static void MapInactiveUserEndpoints(this IEndpointRouteBuilder app) {
		app.MapGet("/api/inactives", (LdapService ldap, HttpRequest request) => {
				if (Authenticator.JwtAuth(request) is { IsSuccessful: false } authResult)
					return authResult;

				IEnumerable<string> employeeUids = ldap.GetAllEntities<Employee>().Values.Where(x => x.IsInactive).Select(x => x.Id);
				IEnumerable<string> studentUids  = ldap.GetAllEntities<Student>().Values.Where(x => x.IsInactive).Select(x => x.Id.ToString());
				string[]            uids         = employeeUids.Concat(studentUids).ToArray();
				return new RequestResult<string>().SetValues(uids).RenewToken(request).ToResult();
			})
			.WithOpenApiJwtAuth()
			.WithTags("Inactive Users")
			.WithDescription("### Returns all UIDs that are part of the \"*inactive*\" group.")
			.Produces<RequestResult<string>>()
			.Produces<RequestResult>(StatusCodes.Status503ServiceUnavailable);
	}
}

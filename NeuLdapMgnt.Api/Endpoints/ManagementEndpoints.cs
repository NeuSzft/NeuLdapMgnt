using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace NeuLdapMgnt.Api.Endpoints;

public static class ManagementEndpoints {
    public static void MapManagementEndpoints(this WebApplication app) {
        app.MapPost("/management/refill", async (LdapHelper ldapHelper, HttpRequest request) => {
               using StreamReader reader = new(request.Body);
               return ldapHelper.TempRefill(await Utils.CsvToStudents(reader), student => student.Id).ToResult();
           })
           .WithOpenApi()
           .WithTags("Management")
           .DisableAntiforgery()
           .Accepts<IFormFile>("text/csv")
           .Produces(StatusCodes.Status207MultiStatus);
    }
}

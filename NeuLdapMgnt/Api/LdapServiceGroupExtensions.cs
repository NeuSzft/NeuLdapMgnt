using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api;

public static class LdapServiceGroupExtensions {
    /// <summary>Checks if the group exists within the database using it's uid.</summary>
    /// <param name="service">The <see cref="LdapService"/> the method should use.</param>
    /// <param name="name">The name of the group.</param>
    /// <returns>Returns <c>true</c> if the group exists. If it does not exist or the search request fails it returns <c>false</c>.</returns>
    public static bool GroupExists(this LdapService service, string name) {
        SearchRequest   request  = new($"ou={name},{service.DnBase}", LdapService.AnyFilter, SearchScope.Base, null);
        SearchResponse? response = service.TryRequest(request) as SearchResponse;

        return response?.Entries.Count == 1;
    }

    /// <summary>Tries to add the group to the database with the specified name.</summary>
    /// <param name="service">The <see cref="LdapService"/> the method should use.</param>
    /// <param name="name">The name of the group.</param>
    /// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
    public static RequestResult TryAddGroup(this LdapService service, string name) {
        if (service.GroupExists(name))
            return new RequestResult().SetStatus(StatusCodes.Status409Conflict).SetErrors("The group already exists.");

        AddRequest request = new($"ou={name},{service.DnBase}", new("organizationalUnit"), new("uidObject"));

        if (service.TryRequest(request, out var error) is not null)
            return new RequestResult().SetStatus(StatusCodes.Status201Created);
        return new RequestResult().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
    }

    /// <summary>Tries to remove an entity from the database with the specified uid.</summary>
    /// <param name="service">The <see cref="LdapService"/> the method should use.</param>
    /// <param name="name">The name of the group.</param>
    /// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
    public static RequestResult TryDeleteGroup(this LdapService service, string name) {
        if (!service.GroupExists(name))
            return new RequestResult().SetStatus(StatusCodes.Status404NotFound).SetErrors("The group does not exist.");

        DeleteRequest request = new($"ou={name},{service.DnBase}");

        if (service.TryRequest(request, out var error) is not null)
            return new RequestResult().SetStatus(StatusCodes.Status200OK);
        return new RequestResult().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
    }

    /// <summary>Tries to add the entity to the group.</summary>
    /// <param name="service">The <see cref="LdapService"/> the method should use.</param>
    /// <param name="name">The name of the group.</param>
    /// <param name="id">The uid of the entity to add.</param>
    /// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
    public static RequestResult TryAddEntityToGroup(this LdapService service, string name, long id) {
        if (!service.GroupExists(name))
            return new RequestResult().SetStatus(StatusCodes.Status404NotFound).SetErrors("The group does not exist.");

        ModifyRequest request = new($"ou={name},{service.DnBase}");

        DirectoryAttributeModification mod = new() {
            Name      = "uid",
            Operation = DirectoryAttributeOperation.Add
        };
        mod.Add(id.ToString());
        request.Modifications.Add(mod);

        if (service.TryRequest(request, out var error) is not null)
            return new RequestResult().SetStatus(StatusCodes.Status201Created);
        return new RequestResult().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
    }

    /// <summary>Tries to add the entities to the group.</summary>
    /// <param name="service">The <see cref="LdapService"/> the method should use.</param>
    /// <param name="name">The name of the group.</param>
    /// <param name="ids">The uids to be added.</param>
    /// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
    public static RequestResult TryAddEntitiesToGroup(this LdapService service, string name, IEnumerable<long> ids) {
        if (!service.GroupExists(name))
            return new RequestResult().SetStatus(StatusCodes.Status404NotFound).SetErrors("The group does not exist.");

        var requests = ids.Select(id => {
            ModifyRequest request = new($"ou={name},{service.DnBase}");

            DirectoryAttributeModification mod = new() {
                Name      = "uid",
                Operation = DirectoryAttributeOperation.Add
            };
            mod.Add(id.ToString());
            request.Modifications.Add(mod);

            return new UniqueDirectoryRequest(request, id.ToString());
        });

        string[] errors = service.TryRequests(requests).Select(x => x.Error).NotNull().ToArray();
        return new RequestResult().SetStatus(StatusCodes.Status207MultiStatus).SetErrors(errors);
    }

    /// <summary>Tries to remove the entity from the group.</summary>
    /// <param name="service">The <see cref="LdapService"/> the method should use.</param>
    /// <param name="name">The name of the group.</param>
    /// <param name="id">The uid of the entity to remove.</param>
    /// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
    public static RequestResult TryRemoveEntityFromGroup(this LdapService service, string name, long id) {
        if (!service.GroupExists(name))
            return new RequestResult().SetStatus(StatusCodes.Status404NotFound).SetErrors("The group does not exist.");

        ModifyRequest request = new($"ou={name},{service.DnBase}");

        DirectoryAttributeModification mod = new() {
            Name      = "uid",
            Operation = DirectoryAttributeOperation.Delete
        };
        mod.Add(id.ToString());
        request.Modifications.Add(mod);

        if (service.TryRequest(request, out var error) is not null)
            return new RequestResult().SetStatus(StatusCodes.Status200OK);
        return new RequestResult().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
    }

    /// <summary>Tries to remove the entities from the group.</summary>
    /// <param name="service">The <see cref="LdapService"/> the method should use.</param>
    /// <param name="name">The name of the group.</param>
    /// <param name="ids">The uids to be removed.</param>
    /// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
    public static RequestResult TryRemoveEntitiesFromGroup(this LdapService service, string name, IEnumerable<long> ids) {
        if (!service.GroupExists(name))
            return new RequestResult().SetStatus(StatusCodes.Status404NotFound).SetErrors("The group does not exist.");

        var requests = ids.Select(id => {
            ModifyRequest request = new($"ou={name},{service.DnBase}");

            DirectoryAttributeModification mod = new() {
                Name      = "uid",
                Operation = DirectoryAttributeOperation.Delete
            };
            mod.Add(id.ToString());
            request.Modifications.Add(mod);

            return new UniqueDirectoryRequest(request, id.ToString());
        });

        string[] errors = service.TryRequests(requests).Select(x => x.Error).NotNull().ToArray();
        return new RequestResult().SetStatus(StatusCodes.Status207MultiStatus).SetErrors(errors);
    }
}

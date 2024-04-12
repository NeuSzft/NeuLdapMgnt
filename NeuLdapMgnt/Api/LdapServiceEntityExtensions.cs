using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api;

public static class LdapServiceEntityExtensions {
    /// <summary>Checks if the entity exists within the database using it's uid.</summary>
    /// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
    /// <param name="id">The uid of the entity.</param>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <returns>Returns <c>true</c> if the entity exists. If it does not exist or the search request fails it returns <c>false</c>.</returns>
    public static bool EntityExists<T>(this LdapService ldap, string id) where T : class {
        SearchRequest   request  = new($"uid={id},ou={typeof(T).GetOuName()},{ldap.DnBase}", LdapService.AnyFilter, SearchScope.Base, null);
        SearchResponse? response = ldap.TryRequest(request) as SearchResponse;

        return response?.Entries.Count == 1;
    }

    /// <summary>Gets all entities of the specified type from the database.</summary>
    /// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <returns>An <see cref="IEnumerable{T}"/> containing the entities.</returns>
    /// <remarks>If an entity cannot be parsed then it is ignored and not returned.</remarks>
    public static IEnumerable<T> GetAllEntities<T>(this LdapService ldap) where T : class, new() {
        SearchRequest   request  = new($"ou={typeof(T).GetOuName()},{ldap.DnBase}", LdapService.AnyFilter, SearchScope.OneLevel, null);
        SearchResponse? response = ldap.TryRequest(request) as SearchResponse;

        if (response is null)
            yield break;

        foreach (SearchResultEntry entry in response.Entries)
            if (LdapService.TryParseEntry<T>(entry, out _) is { } entity)
                yield return entity;
    }

    /// <summary>Tries to get the entity from the database using it's uid.</summary>
    /// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
    /// <param name="id">The uid of the entity.</param>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <returns>A <see cref="RequestResult{T}"/> containing the outcome of the operation.</returns>
    public static RequestResult<T> TryGetEntity<T>(this LdapService ldap, string id) where T : class, new() {
        SearchRequest   request  = new($"uid={id},ou={typeof(T).GetOuName()},{ldap.DnBase}", LdapService.AnyFilter, SearchScope.Base, null);
        SearchResponse? response = ldap.TryRequest(request, out var error) as SearchResponse;

        if (response is null || response.Entries.Count == 0)
            return new RequestResult<T>().SetStatus(StatusCodes.Status404NotFound).SetErrors(error ?? "The object does not exist.");

        T? entity = LdapService.TryParseEntry<T>(response.Entries[0], out error);
        if (entity is not null)
            return new RequestResult<T>().SetStatus(StatusCodes.Status200OK).SetValues(entity);
        return new RequestResult<T>().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
    }

    /// <summary>Tries to add the entity to the database with the specified uid.</summary>
    /// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
    /// <param name="id">The uid of the entity.</param>
    /// <param name="entity">The entity to be added.</param>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <returns>A <see cref="RequestResult{T}"/> containing the outcome of the operation.</returns>
    public static RequestResult<T> TryAddEntity<T>(this LdapService ldap, T entity, string id) where T : class {
        Type type = typeof(T);

        ldap.TryRequest(new AddRequest($"ou={type.GetOuName()},{ldap.DnBase}", "organizationalUnit"));

        if (ldap.EntityExists<T>(id))
            return new RequestResult<T>().SetStatus(StatusCodes.Status409Conflict).SetErrors("The object already exists.");

        AddRequest request = new($"uid={id},ou={type.GetOuName()},{ldap.DnBase}");
        if (type.GetCustomAttribute<LdapObjectClassesAttribute>() is { } objectClasses)
            request.Attributes.Add(new("objectClass", objectClasses.Classes.Cast<object>().ToArray()));

        foreach (DirectoryAttribute attribute in LdapService.GetDirectoryAttributes(entity))
            request.Attributes.Add(attribute);

        if (ldap.TryRequest(request, out var error) is not null)
            return new RequestResult<T>().SetStatus(StatusCodes.Status201Created).SetValues(entity);
        return new RequestResult<T>().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
    }

    /// <summary>Tries to modify an entity in the database with the specified uid.</summary>
    /// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
    /// <param name="id">The uid of the entity to replace.</param>
    /// <param name="entity">The entity that will replace the current one.</param>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <returns>A <see cref="RequestResult{T}"/> containing the outcome of the operation.</returns>
    public static RequestResult<T> TryModifyEntity<T>(this LdapService ldap, T entity, string id) where T : class {
        Type type = typeof(T);

        if (!ldap.EntityExists<T>(id))
            return new RequestResult<T>().SetStatus(StatusCodes.Status404NotFound).SetErrors("The object does not exist.");

        ModifyRequest request = new($"uid={id},ou={typeof(T).GetOuName()},{ldap.DnBase}");

        foreach (PropertyInfo info in type.GetProperties())
            if (info.GetCustomAttribute(typeof(LdapAttributeAttribute)) is LdapAttributeAttribute attribute) {
                DirectoryAttributeModification mod = new() {
                    Name      = attribute.Name,
                    Operation = DirectoryAttributeOperation.Replace,
                };
                mod.Add(info.GetValue(entity)?.ToString());
                request.Modifications.Add(mod);
            }

        if (ldap.TryRequest(request, out var error) is not null)
            return new RequestResult<T>().SetStatus(StatusCodes.Status200OK).SetValues(entity);
        return new RequestResult<T>().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
    }

    /// <summary>Tries to remove an entity from the database with the specified uid.</summary>
    /// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
    /// <param name="id">The uid of the entity to remove.</param>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
    public static RequestResult TryDeleteEntity<T>(this LdapService ldap, string id) where T : class {
        if (!ldap.EntityExists<T>(id))
            return new RequestResult().SetStatus(StatusCodes.Status404NotFound).SetErrors("The object does not exist.");

        DeleteRequest request = new($"uid={id},ou={typeof(T).GetOuName()},{ldap.DnBase}");

        if (ldap.TryRequest(request, out var error) is not null)
            return new RequestResult().SetStatus(StatusCodes.Status200OK);
        return new RequestResult().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
    }

    /// <summary>Tries to add the entities to the database.</summary>
    /// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
    /// <param name="entities">The entities to be added.</param>
    /// <param name="idGetter">A delegate that takes in an entity of type <typeparamref name="T"/> and returns an uid with the type of <c>long</c>.</param>
    /// <typeparam name="T">The type of the entities.</typeparam>
    /// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
    public static RequestResult TryAddEntities<T>(this LdapService ldap, IEnumerable<T> entities, Func<T, string> idGetter) where T : class {
        Type type = typeof(T);

        ldap.TryRequest(new AddRequest($"ou={type.GetOuName()},{ldap.DnBase}", "organizationalUnit"));

        DirectoryAttribute? objectClassesAttribute = null;
        if (type.GetCustomAttribute<LdapObjectClassesAttribute>() is { } objectClasses)
            objectClassesAttribute = new("objectClass", objectClasses.Classes.Cast<object>().ToArray());

        var requests = entities.Select(x => {
            string id = idGetter(x);

            AddRequest request = new($"uid={id},ou={type.GetOuName()},{ldap.DnBase}");
            if (objectClassesAttribute is not null)
                request.Attributes.Add(objectClassesAttribute);

            foreach (DirectoryAttribute attribute in LdapService.GetDirectoryAttributes(x))
                request.Attributes.Add(attribute);

            return new UniqueDirectoryRequest(request, id.ToString());
        });

        string[] errors = ldap.TryRequests(requests).Select(x => x.Error).NotNull().ToArray();
        return new RequestResult().SetStatus(StatusCodes.Status207MultiStatus).SetErrors(errors);
    }
}

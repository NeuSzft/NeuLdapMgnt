using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api;

public static class LdapHelperExtensions {
    /// <summary>Checks if the entity exists within the database using it's uid.</summary>
    /// <param name="helper">The <see cref="LdapHelper"/> the method should use.</param>
    /// <param name="id">The uid of the entity.</param>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <returns>Returns <c>true</c> if the entity exists. If it does not exist or the search request fails it returns <c>false</c>.</returns>
    public static bool EntityExists<T>(this LdapHelper helper, long id) where T : class {
        SearchRequest   request  = new($"uid={id},ou={typeof(T).GetOuName()},{helper.DnBase}", LdapHelper.AnyFilter, SearchScope.Base, null);
        SearchResponse? response = helper.TryRequest(request) as SearchResponse;

        return response?.Entries.Count == 1;
    }

    /// <summary>Gets all entities of the specified type from the database.</summary>
    /// <param name="helper">The <see cref="LdapHelper"/> the method should use.</param>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <returns>An <see cref="IEnumerable{T}"/> containing the entities.</returns>
    /// <remarks>If an entity cannot be parsed then it is ignored and not returned.</remarks>
    public static IEnumerable<T> GetAllEntities<T>(this LdapHelper helper) where T : class, new() {
        SearchRequest   request  = new($"ou={typeof(T).GetOuName()},{helper.DnBase}", LdapHelper.AnyFilter, SearchScope.Subtree, null);
        SearchResponse? response = helper.TryRequest(request) as SearchResponse;

        if (response is null)
            yield break;

        foreach (SearchResultEntry entry in response.Entries)
            if (LdapHelper.TryParseEntry<T>(entry, out _) is { } entity)
                yield return entity;
    }

    /// <summary>Tries to get the entity from the database using it's uid.</summary>
    /// <param name="helper">The <see cref="LdapHelper"/> the method should use.</param>
    /// <param name="id">The uid of the entity.</param>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <returns>A <see cref="RequestResult{T}"/> containing the outcome of the operation.</returns>
    public static RequestResult<T> TryGetEntity<T>(this LdapHelper helper, long id) where T : class, new() {
        SearchRequest   request  = new($"uid={id},ou={typeof(T).GetOuName()},{helper.DnBase}", LdapHelper.AnyFilter, SearchScope.Base, null);
        SearchResponse? response = helper.TryRequest(request, out var error) as SearchResponse;

        if (response is null || response.Entries.Count == 0)
            return new RequestResult<T>().SetStatus(StatusCodes.Status404NotFound).SetErrors(error ?? "The object does not exist.");

        T? entity = LdapHelper.TryParseEntry<T>(response.Entries[0], out error);
        if (entity is not null)
            return new RequestResult<T>().SetStatus(StatusCodes.Status200OK).SetValues(entity);
        return new RequestResult<T>().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
    }

    /// <summary>Tries to add the entity to the database with the specified uid.</summary>
    /// <param name="helper">The <see cref="LdapHelper"/> the method should use.</param>
    /// <param name="id">The uid of the entity.</param>
    /// <param name="entity">The entity to be added.</param>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <returns>A <see cref="RequestResult{T}"/> containing the outcome of the operation.</returns>
    public static RequestResult<T> TryAddEntity<T>(this LdapHelper helper, T entity, long id) where T : class {
        Type type = typeof(T);

        helper.TryRequest(new AddRequest($"ou={type.GetOuName()},{helper.DnBase}", "organizationalUnit"));

        if (helper.EntityExists<T>(id))
            return new RequestResult<T>().SetStatus(StatusCodes.Status409Conflict).SetErrors("The object already exists.");

        AddRequest request = new($"uid={id},ou={type.GetOuName()},{helper.DnBase}");
        if (type.GetCustomAttribute<LdapObjectClassesAttribute>() is { } objectClasses)
            request.Attributes.Add(new("objectClass", objectClasses.Classes.Cast<object>().ToArray()));

        foreach (DirectoryAttribute attribute in LdapHelper.GetDirectoryAttributes(entity))
            request.Attributes.Add(attribute);

        if (helper.TryRequest(request, out var error) is not null)
            return new RequestResult<T>().SetStatus(StatusCodes.Status201Created).SetValues(entity);
        return new RequestResult<T>().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
    }

    /// <summary>Tries to modify an entity in the database with the specified uid.</summary>
    /// <param name="helper">The <see cref="LdapHelper"/> the method should use.</param>
    /// <param name="id">The uid of the entity to replace.</param>
    /// <param name="entity">The entity that will replace the current one.</param>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <returns>A <see cref="RequestResult{T}"/> containing the outcome of the operation.</returns>
    public static RequestResult<T> TryModifyEntity<T>(this LdapHelper helper, T entity, long id) where T : class {
        Type type = typeof(T);

        if (!helper.EntityExists<T>(id))
            return new RequestResult<T>().SetStatus(StatusCodes.Status404NotFound).SetErrors("The object does not exist.");

        ModifyRequest request = new($"uid={id},ou={typeof(T).GetOuName()},{helper.DnBase}");

        foreach (PropertyInfo info in type.GetProperties())
            if (info.GetCustomAttribute(typeof(LdapAttributeAttribute)) is LdapAttributeAttribute attribute) {
                DirectoryAttributeModification mod = new() {
                    Name      = attribute.Name,
                    Operation = DirectoryAttributeOperation.Replace,
                };
                mod.Add(info.GetValue(entity)?.ToString());
                request.Modifications.Add(mod);
            }

        if (helper.TryRequest(request, out var error) is not null)
            return new RequestResult<T>().SetStatus(StatusCodes.Status200OK).SetValues(entity);
        return new RequestResult<T>().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
    }

    /// <summary>Tries to remove an entity from the database with the specified uid.</summary>
    /// <param name="helper">The <see cref="LdapHelper"/> the method should use.</param>
    /// <param name="id">The uid of the entity to remove.</param>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
    public static RequestResult TryDeleteEntity<T>(this LdapHelper helper, long id) where T : class {
        if (!helper.EntityExists<T>(id))
            return new RequestResult().SetStatus(StatusCodes.Status404NotFound).SetErrors("The object does not exist.");

        DeleteRequest request = new($"uid={id},ou={typeof(T).GetOuName()},{helper.DnBase}");

        if (helper.TryRequest(request, out var error) is not null)
            return new RequestResult().SetStatus(StatusCodes.Status200OK);
        return new RequestResult().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
    }

    /// <summary>Tries to add the entities to the database.</summary>
    /// <param name="helper">The <see cref="LdapHelper"/> the method should use.</param>
    /// <param name="entities">The entities to be added.</param>
    /// <param name="idGetter">A delegate that takes in an entity of type <typeparamref name="T"/> and returns an uid with the type of <c>long</c>.</param>
    /// <typeparam name="T">The type of the entities.</typeparam>
    /// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
    public static RequestResult TryAddEntities<T>(this LdapHelper helper, IEnumerable<T> entities, Func<T, long> idGetter) where T : class {
        Type type = typeof(T);

        helper.TryRequest(new AddRequest($"ou={type.GetOuName()},{helper.DnBase}", "organizationalUnit"));

        DirectoryAttribute? objectClassesAttribute = null;
        if (type.GetCustomAttribute<LdapObjectClassesAttribute>() is { } objectClasses)
            objectClassesAttribute = new("objectClass", objectClasses.Classes.Cast<object>().ToArray());

        var requests = entities.Select(x => {
            long id = idGetter(x);

            AddRequest request = new($"uid={id},ou={type.GetOuName()},{helper.DnBase}");
            if (objectClassesAttribute is not null)
                request.Attributes.Add(objectClassesAttribute);

            foreach (DirectoryAttribute attribute in LdapHelper.GetDirectoryAttributes(x))
                request.Attributes.Add(attribute);

            return (request as DirectoryRequest, (string?)id.ToString());
        });

        string[] errors = helper.TryRequests(requests).Select(x => x.Error).NotNull().ToArray();
        return new RequestResult().SetStatus(StatusCodes.Status207MultiStatus).SetErrors(errors);
    }
}

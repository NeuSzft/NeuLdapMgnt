using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api;

public static class LdapHelperExtensions {
    public static bool EntityExists<T>(this LdapHelper helper, long id) where T : class {
        SearchRequest   request  = new($"uid={id},ou={typeof(T).GetOuName()},{helper.DnBase}", LdapHelper.AnyFilter, SearchScope.Base, null);
        SearchResponse? response = helper.TryRequest(request) as SearchResponse;

        return response?.Entries.Count == 1;
    }

    public static IEnumerable<T> GetAllEntities<T>(this LdapHelper helper) where T : class, new() {
        SearchRequest   request  = new($"ou={typeof(T).GetOuName()},{helper.DnBase}", LdapHelper.AnyFilter, SearchScope.Subtree, null);
        SearchResponse? response = helper.TryRequest(request) as SearchResponse;

        if (response is null)
            yield break;

        foreach (SearchResultEntry entry in response.Entries)
            if (LdapHelper.TryParseEntry<T>(entry, out _) is { } entity)
                yield return entity;
    }

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

    public static RequestResult<T> TryAddEntity<T>(this LdapHelper helper, T entity, long id) where T : class {
        Type type = typeof(T);

        helper.TryRequest(new AddRequest($"ou={type.GetOuName()},{helper.DnBase}", "organizationalUnit"));

        if (helper.EntityExists<T>(id))
            return new RequestResult<T>().SetStatus(StatusCodes.Status409Conflict).SetErrors("The object already exists.");

        AddRequest request = new($"uid={id},ou={type.GetOuName()},{helper.DnBase}");
        if (type.GetCustomAttribute<LdapObjectClassesAttribute>() is { } objectClasses)
            request.Attributes.Add(new("objectClass", objectClasses.Classes.Cast<object>().ToArray()));

        foreach (DirectoryAttribute attribute in LdapHelper.GetDirectoryAttribute(entity))
            request.Attributes.Add(attribute);

        if (helper.TryRequest(request, out var error) is not null)
            return new RequestResult<T>().SetStatus(StatusCodes.Status201Created).SetValues(entity);
        return new RequestResult<T>().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
    }

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

    public static RequestResult TryDeleteEntity<T>(this LdapHelper helper, long id) where T : class {
        if (!helper.EntityExists<T>(id))
            return new RequestResult().SetStatus(StatusCodes.Status404NotFound).SetErrors("The object does not exist.");

        DeleteRequest request = new($"uid={id},ou={typeof(T).GetOuName()},{helper.DnBase}");

        if (helper.TryRequest(request, out var error) is not null)
            return new RequestResult().SetStatus(StatusCodes.Status200OK);
        return new RequestResult().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
    }

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

            foreach (DirectoryAttribute attribute in LdapHelper.GetDirectoryAttribute(x))
                request.Attributes.Add(attribute);

            return (request as DirectoryRequest, (string?)id.ToString());
        });

        string[] errors = helper.TryRequests(requests).Select(x => x.Error).NotNull().ToArray();
        return new RequestResult().SetStatus(StatusCodes.Status207MultiStatus).SetErrors(errors);
    }
}

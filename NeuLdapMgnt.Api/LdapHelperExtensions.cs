using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api;

public record LdapOperationResult(int Code, string? Message) {
    public virtual IResult ToResult() {
        return Results.Text(Message, "text/plain", Encoding.UTF8, Code);
    }
}

public record LdapOperationResult<T>(int Code, string? Message, T? Value = default) : LdapOperationResult(Code, Message) {
    public override IResult ToResult() {
        if (Code is >= 200 and <= 299)
            return Results.Json(Value, JsonSerializerOptions.Default, "application/json", Code);
        return base.ToResult();
    }
}

public record MultiStatusResponse(int Successful, int Failed, IEnumerable<string> Errors) {
    public IResult ToResult() {
        var data = new {
            successful = Successful,
            failed     = Failed,
            errors     = Errors
        };
        return Results.Json(data, new JsonSerializerOptions { WriteIndented = true }, "application/json", StatusCodes.Status207MultiStatus);
    }
}

public static class LdapHelperExtensions {
    public static bool EntityExists<T>(this LdapHelper helper, long id) where T : class {
        SearchRequest   request  = new($"uid={id},ou={typeof(T).GetOuName()},{helper.DnBase}", LdapHelper.AnyFilter, SearchScope.Base, null);
        SearchResponse? response = helper.TryRequest(request) as SearchResponse;

        return response?.Entries.Count == 1;
    }

    public static IEnumerable<T> GetAllEntities<T>(this LdapHelper helper) where T : class {
        SearchRequest   request  = new($"ou={typeof(T).GetOuName()},{helper.DnBase}", LdapHelper.AnyFilter, SearchScope.Subtree, null);
        SearchResponse? response = helper.TryRequest(request) as SearchResponse;

        if (response is null)
            yield break;

        foreach (SearchResultEntry entry in response.Entries)
            if (LdapHelper.TryParseEntry<T>(entry, out _) is { } entity)
                yield return entity;
    }

    public static LdapOperationResult<T> TryGetEntity<T>(this LdapHelper helper, long id) where T : class {
        SearchRequest   request  = new($"uid={id},ou={typeof(T).GetOuName()},{helper.DnBase}", LdapHelper.AnyFilter, SearchScope.Base, null);
        SearchResponse? response = helper.TryRequest(request, out var error) as SearchResponse;

        if (response is null || response.Entries.Count == 0)
            return new(StatusCodes.Status404NotFound, error);

        T? entity = LdapHelper.TryParseEntry<T>(response.Entries[0], out error);
        return new(entity is null ? StatusCodes.Status400BadRequest : StatusCodes.Status200OK, error, entity);
    }

    public static LdapOperationResult TryAddEntity<T>(this LdapHelper helper, T entity, long id) where T : class {
        if (helper.EntityExists<T>(id))
            return new(StatusCodes.Status409Conflict, "The object already exists.");

        Type type = typeof(T);

        AddRequest request = new($"uid={id},ou={type.GetOuName()},{helper.DnBase}");
        if (type.GetCustomAttribute<LdapObjectClassesAttribute>() is { } objectClasses)
            request.Attributes.Add(new("objectClass", objectClasses.Classes.Cast<object>().ToArray()));

        foreach (DirectoryAttribute attribute in LdapHelper.GetDirectoryAttribute(entity))
            request.Attributes.Add(attribute);

        bool created = helper.TryRequest(request, out var error) is not null;
        return new(created ? StatusCodes.Status201Created : StatusCodes.Status400BadRequest, error);
    }

    public static LdapOperationResult TryModifyEntity<T>(this LdapHelper helper, T entity, long id) where T : class {
        if (!helper.EntityExists<T>(id))
            return new(StatusCodes.Status404NotFound, "The object does not exist.");

        ModifyRequest request = new($"uid={id},ou={typeof(T).GetOuName()},{helper.DnBase}");

        Type type = entity.GetType();
        foreach (PropertyInfo info in type.GetProperties())
            if (info.GetCustomAttribute(typeof(LdapAttributeAttribute)) is LdapAttributeAttribute attribute) {
                DirectoryAttributeModification mod = new() {
                    Name      = attribute.Name,
                    Operation = DirectoryAttributeOperation.Replace,
                };
                mod.Add(info.GetValue(entity)?.ToString());
                request.Modifications.Add(mod);
            }

        bool modified = helper.TryRequest(request, out var error) is not null;
        return new(modified ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest, error);
    }

    public static LdapOperationResult TryDeleteEntity<T>(this LdapHelper helper, long id) where T : class {
        if (!helper.EntityExists<T>(id))
            return new(StatusCodes.Status404NotFound, "The object does not exist.");

        DeleteRequest request = new($"uid={id},ou={typeof(T).GetOuName()},{helper.DnBase}");

        bool deleted = helper.TryRequest(request, out var error) is not null;
        return new(deleted ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest, error);
    }

    public static MultiStatusResponse TempRefill<T>(this LdapHelper helper, IEnumerable<T> items, Func<T, long> idGetter) where T : class {
        Type type = typeof(T);

        DirectoryAttribute? objectClassesAttribute = null;
        if (type.GetCustomAttribute<LdapObjectClassesAttribute>() is { } objectClasses)
            objectClassesAttribute = new("objectClass", objectClasses.Classes.Cast<object>().ToArray());

        int          completed = 0;
        List<string> errors    = new();

        foreach (T entity in items) {
            long id = idGetter(entity);

            AddRequest request = new($"uid={id},ou={type.GetOuName()},{helper.DnBase}");
            if (objectClassesAttribute is not null)
                request.Attributes.Add(objectClassesAttribute);

            foreach (DirectoryAttribute attribute in LdapHelper.GetDirectoryAttribute(entity))
                request.Attributes.Add(attribute);

            if (helper.TryRequest(request, out var error) is not null)
                completed++;
            if (error is not null)
                errors.Add($"{id}: {error}");
        }

        return new(completed, items.Count() - completed, errors);
    }
}

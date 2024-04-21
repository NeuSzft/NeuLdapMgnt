using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.LdapServiceExtensions;

public static class EntityExtensions {
	/// <summary>Checks if the entity exists within the database using it's uid.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="id">The uid of the entity.</param>
	/// <typeparam name="T">The type of the entity.</typeparam>
	/// <returns>Returns <c>true</c> if the entity exists. If it does not exist or the search request fails it returns <c>false</c>.</returns>
	public static bool EntityExists<T>(this LdapService ldap, string id) where T : class {
		SearchRequest   request  = new($"uid={id},ou={typeof(T).GetOuName()},{ldap.DomainComponents}", LdapService.AnyFilter, SearchScope.Base, []);
		SearchResponse? response = ldap.TryRequest(request) as SearchResponse;

		return response?.Entries.Count == 1;
	}

	/// <summary>Gets all entities of the specified type from the database.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="getHiddenAttributes">If <c>true</c> even the attributes that are set to be hidden are returned.</param>
	/// <typeparam name="T">The type of the entity.</typeparam>
	/// <returns>A <see cref="RequestResult{T}"/> containing the entities.</returns>
	public static RequestResult<T> GetAllEntities<T>(this LdapService ldap, bool getHiddenAttributes = false) where T : class, new() {
		Type type = typeof(T);

		SearchRequest   request  = new($"ou={type.GetOuName()},{ldap.DomainComponents}", LdapService.AnyFilter, SearchScope.OneLevel, null);
		SearchResponse? response = ldap.TryRequest(request) as SearchResponse;

		if (response is null)
			return new RequestResult<T>().SetStatus(StatusCodes.Status503ServiceUnavailable).SetErrors($"Failed to fetch entities of type {type.Name}.");

		List<T>      entities = new();
		List<string> errors   = new();

		foreach (SearchResultEntry entry in response.Entries) {
			if (LdapService.TryParseEntry<T>(entry, out var error, getHiddenAttributes) is { } entity)
				entities.Add(entity);
			if (error is not null)
				errors.Add(error);
		}

		return new RequestResult<T>().SetStatus(StatusCodes.Status207MultiStatus).SetValues(entities.ToArray()).SetErrors(errors.ToArray());
	}

	/// <summary>Tries to get the entity from the database using it's uid.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="id">The uid of the entity.</param>
	/// <param name="getHiddenAttributes">If <c>true</c> even the attributes that are set to be hidden are returned.</param>
	/// <typeparam name="T">The type of the entity.</typeparam>
	/// <returns>A <see cref="RequestResult{T}"/> containing the outcome of the operation.</returns>
	public static RequestResult<T> TryGetEntity<T>(this LdapService ldap, string id, bool getHiddenAttributes = false) where T : class, new() {
		SearchRequest   request  = new($"uid={id},ou={typeof(T).GetOuName()},{ldap.DomainComponents}", LdapService.AnyFilter, SearchScope.Base, null);
		SearchResponse? response = ldap.TryRequest(request, out var error) as SearchResponse;

		if (response is null || response.Entries.Count == 0)
			return new RequestResult<T>().SetStatus(StatusCodes.Status404NotFound).SetErrors(error ?? "The object does not exist.");

		T? entity = LdapService.TryParseEntry<T>(response.Entries[0], out error, getHiddenAttributes);
		if (entity is not null)
			return new RequestResult<T>().SetStatus(StatusCodes.Status200OK).SetValues(entity);
		return new RequestResult<T>().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
	}

	/// <summary>Tries to add the entity to the database with the specified uid.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="entity">The entity to be added.</param>
	/// <param name="id">The uid of the entity.</param>
	/// <param name="setHiddenAttributes">If <c>true</c> even the attributes that are set to be hidden are set.</param>
	/// <typeparam name="T">The type of the entity.</typeparam>
	/// <returns>A <see cref="RequestResult{T}"/> containing the outcome of the operation.</returns>
	public static RequestResult<T> TryAddEntity<T>(this LdapService ldap, T entity, string id, bool setHiddenAttributes = false) where T : class {
		Type type = typeof(T);

		ldap.TryRequest(new AddRequest($"ou={type.GetOuName()},{ldap.DomainComponents}", "organizationalUnit"));

		if (ldap.EntityExists<T>(id))
			return new RequestResult<T>().SetStatus(StatusCodes.Status409Conflict).SetErrors("The object already exists.");

		AddRequest request = new($"uid={id},ou={type.GetOuName()},{ldap.DomainComponents}");
		if (type.GetCustomAttribute<LdapObjectClassesAttribute>() is { } objectClasses)
			request.Attributes.Add(new("objectClass", objectClasses.Classes.Cast<object>().ToArray()));

		foreach (DirectoryAttribute attribute in LdapService.GetDirectoryAttributes(entity, setHiddenAttributes))
			request.Attributes.Add(attribute);

		if (ldap.TryRequest(request, out var error) is not null)
			return new RequestResult<T>().SetStatus(StatusCodes.Status201Created).SetValues(entity);
		return new RequestResult<T>().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
	}

	/// <summary>Tries to modify an entity in the database with the specified uid.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="entity">The entity that will replace the current one.</param>
	/// <param name="id">The uid of the entity to replace.</param>
	/// <param name="setHiddenAttributes">If <c>true</c> even the attributes that are set to be hidden are set.</param>
	/// <typeparam name="T">The type of the entity.</typeparam>
	/// <returns>A <see cref="RequestResult{T}"/> containing the outcome of the operation.</returns>
	public static RequestResult<T> TryModifyEntity<T>(this LdapService ldap, T entity, string id, bool setHiddenAttributes = false) where T : class {
		Type type = typeof(T);

		if (!ldap.EntityExists<T>(id))
			return new RequestResult<T>().SetStatus(StatusCodes.Status404NotFound).SetErrors("The object does not exist.");

		ModifyRequest request = new($"uid={id},ou={typeof(T).GetOuName()},{ldap.DomainComponents}");

		foreach (PropertyInfo info in type.GetProperties())
			if (info.GetCustomAttribute(typeof(LdapAttributeAttribute)) is LdapAttributeAttribute attribute && (!attribute.Hidden || setHiddenAttributes)) {
				DirectoryAttributeModification mod = new() {
					Name = attribute.Name,
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

		DeleteRequest request = new($"uid={id},ou={typeof(T).GetOuName()},{ldap.DomainComponents}");

		if (ldap.TryRequest(request, out var error) is not null)
			return new RequestResult().SetStatus(StatusCodes.Status200OK);
		return new RequestResult().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
	}

	/// <summary>Tries to add the entities to the database.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="entities">The entities to be added.</param>
	/// <param name="idGetter">A delegate that takes in an entity of type <typeparamref name="T"/> and returns an uid with the type of <c>long</c>.</param>
	/// <param name="setHiddenAttributes">If <c>true</c> even the attributes that are set to be hidden are set.</param>
	/// <typeparam name="T">The type of the entities.</typeparam>
	/// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
	public static RequestResult TryAddEntities<T>(this LdapService ldap, IEnumerable<T> entities, Func<T, string> idGetter, bool setHiddenAttributes = false) where T : class {
		Type type = typeof(T);

		ldap.TryRequest(new AddRequest($"ou={type.GetOuName()},{ldap.DomainComponents}", "organizationalUnit"));

		DirectoryAttribute? objectClassesAttribute = null;
		if (type.GetCustomAttribute<LdapObjectClassesAttribute>() is { } objectClasses)
			objectClassesAttribute = new("objectClass", objectClasses.Classes.Cast<object>().ToArray());

		var requests = entities.Select(x => {
			string id = idGetter(x);

			AddRequest request = new($"uid={id},ou={type.GetOuName()},{ldap.DomainComponents}");
			if (objectClassesAttribute is not null)
				request.Attributes.Add(objectClassesAttribute);

			foreach (DirectoryAttribute attribute in LdapService.GetDirectoryAttributes(x, setHiddenAttributes))
				request.Attributes.Add(attribute);

			return new UniqueDirectoryRequest(request, id.ToString());
		});

		string[] errors = ldap.TryRequests(requests).Select(x => x.Error).NotNull().ToArray();
		return new RequestResult().SetStatus(StatusCodes.Status207MultiStatus).SetErrors(errors);
	}

	/// <summary>Tries to get the display name (or full name) of an entity.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="id">The uid of the entity to find.</param>
	/// <param name="typesToTry">The model types to search trough.</param>
	/// <returns>The <c>displayName</c> attribute of the entity or <c>null</c> if not found.</returns>
	public static string? TryGetDisplayNameOfEntity(this LdapService ldap, string id, params Type[] typesToTry) {
		if (id == Authenticator.GetDefaultAdminName())
			return null;

		foreach (Type type in typesToTry) {
			SearchRequest   request  = new($"uid={id},ou={type.GetOuName()},{ldap.DomainComponents}", LdapService.AnyFilter, SearchScope.Base, "displayName");
			SearchResponse? response = ldap.TryRequest(request) as SearchResponse;

			try {
				return response!.Entries[0].Attributes["displayName"][0].ToString();
			}
			catch {
				continue;
			}
		}

		return null;
	}
	
	/// <summary>Tries to get the password hash and salt of an entity.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="id">The uid of the entity to find.</param>
	/// <param name="typesToTry">The model types to search trough.</param>
	/// <returns>The <c>userPassword</c> attribute of the entity or <c>null</c> if not found.</returns>
	public static string? TryGetPasswordOfEntity(this LdapService ldap, string id, params Type[] typesToTry) {
		if (id == Authenticator.GetDefaultAdminName())
			return ldap.GetValue(Authenticator.DefaultAdminPasswordValueName, out _);

		foreach (Type type in typesToTry) {
			SearchRequest   request  = new($"uid={id},ou={type.GetOuName()},{ldap.DomainComponents}", LdapService.AnyFilter, SearchScope.Base, "userPassword");
			SearchResponse? response = ldap.TryRequest(request) as SearchResponse;

			try {
				return response!.Entries[0].Attributes["userPassword"][0].ToString();
			}
			catch {
				continue;
			}
		}

		return null;
	}
}

using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.LdapServiceExtensions;

public static class EntityExtensions {
	/// <summary>Checks if the entity exists within the database using its uid.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="id">The uid of the entity.</param>
	/// <typeparam name="T">The type of the entity.</typeparam>
	/// <returns>Returns <c>true</c> if the entity exists. If it does not exist or the search request fails it returns <c>false</c>.</returns>
	public static bool EntityExists<T>(this LdapService ldap, string id) where T : class {
		SearchRequest request  = new($"uid={id},ou={typeof(T).GetOuName()},{ldap.DomainComponents}", LdapService.AnyFilter, SearchScope.Base, [ ]);
		var           response = ldap.TryRequest(request) as SearchResponse;

		return response?.Entries.Count == 1;
	}

	/// <summary>Returns the attributes of the entity that are present within the database using its uid.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="id">The uid of the entity.</param>
	/// <typeparam name="T">The type of the entity.</typeparam>
	/// <returns>Returns the LDAP attributes of the entity that are present within the database or an empty collection if it is not found.</returns>
	public static IEnumerable<string> TryGetPresentEntityAttributes<T>(this LdapService ldap, string id) where T : class {
		SearchRequest request  = new($"uid={id},ou={typeof(T).GetOuName()},{ldap.DomainComponents}", LdapService.AnyFilter, SearchScope.Base, null);
		var           response = ldap.TryRequest(request) as SearchResponse;

		return response?.Entries.Count > 0
			? response.Entries[0].Attributes.Values.Cast<DirectoryAttribute>().Select(x => x.Name)
			: [ ];
	}

	/// <summary>Gets all entities of the specified type from the database.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="getHiddenAttributes">If <c>true</c> even the attributes that are set to be hidden are returned.</param>
	/// <typeparam name="T">The type of the entity.</typeparam>
	/// <returns>A <see cref="RequestResult{T}"/> containing the entities.</returns>
	public static RequestResult<T> GetAllEntities<T>(this LdapService ldap, bool getHiddenAttributes = false) where T : class, new() {
		var type = typeof(T);

		ldap.TryRequest(new AddRequest($"ou={type.GetOuName()},{ldap.DomainComponents}", "organizationalUnit"), false);

		SearchRequest request = new($"ou={type.GetOuName()},{ldap.DomainComponents}", LdapService.AnyFilter, SearchScope.OneLevel, null);
		if (ldap.TryRequest(request) is not SearchResponse response)
			return new RequestResult<T>().SetStatus(StatusCodes.Status503ServiceUnavailable).SetErrors($"Failed to fetch entities of type {type.Name}.");

		List<T>      entities = [ ];
		List<string> errors   = [ ];

		foreach (SearchResultEntry entry in response.Entries) {
			if (LdapService.TryParseEntry<T>(entry, out string? error, getHiddenAttributes) is { } entity)
				entities.Add(entity);
			if (error is not null)
				errors.Add(error);
		}

		return new RequestResult<T>().SetStatus(StatusCodes.Status207MultiStatus).SetValues(entities.ToArray()).SetErrors(errors.ToArray());
	}

	/// <summary>Tries to get the entity from the database using its uid.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="id">The uid of the entity.</param>
	/// <param name="getHiddenAttributes">If <c>true</c> even the attributes that are set to be hidden are returned.</param>
	/// <typeparam name="T">The type of the entity.</typeparam>
	/// <returns>A <see cref="RequestResult{T}"/> containing the outcome of the operation.</returns>
	public static RequestResult<T> TryGetEntity<T>(this LdapService ldap, string id, bool getHiddenAttributes = false) where T : class, new() {
		SearchRequest request = new($"uid={id},ou={typeof(T).GetOuName()},{ldap.DomainComponents}", LdapService.AnyFilter, SearchScope.Base, null);

		if (ldap.TryRequest(request, out string? error) is not SearchResponse response || response.Entries.Count == 0)
			return new RequestResult<T>().SetStatus(StatusCodes.Status404NotFound).SetErrors(error ?? "The object does not exist.");

		var entity = LdapService.TryParseEntry<T>(response.Entries[0], out error, getHiddenAttributes);
		return entity is not null
			? new RequestResult<T>().SetStatus(StatusCodes.Status200OK).SetValues(entity)
			: new RequestResult<T>().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
	}

	/// <summary>Tries to add the entity to the database with the specified uid.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="entity">The entity to be added.</param>
	/// <param name="id">The uid of the entity.</param>
	/// <param name="setHiddenAttributes">If <c>true</c> even the attributes that are set to be hidden are set.</param>
	/// <typeparam name="T">The type of the entity.</typeparam>
	/// <returns>A <see cref="RequestResult{T}"/> containing the outcome of the operation.</returns>
	public static RequestResult<T> TryAddEntity<T>(this LdapService ldap, T entity, string id, bool setHiddenAttributes = false) where T : class {
		var type = typeof(T);

		ldap.TryRequest(new AddRequest($"ou={type.GetOuName()},{ldap.DomainComponents}", "organizationalUnit"), false);

		if (ldap.EntityExists<T>(id))
			return new RequestResult<T>().SetStatus(StatusCodes.Status409Conflict).SetErrors("The object already exists.");

		AddRequest request = new($"uid={id},ou={type.GetOuName()},{ldap.DomainComponents}");
		if (type.GetCustomAttribute<LdapObjectClassesAttribute>() is { } objectClasses)
			request.Attributes.Add(new DirectoryAttribute("objectClass", objectClasses.Classes.Cast<object>().ToArray()));

		foreach (var attribute in LdapService.GetDirectoryAttributes(entity, setHiddenAttributes))
			request.Attributes.Add(attribute);

		return ldap.TryRequest(request, out string? error) is not null
			? new RequestResult<T>().SetStatus(StatusCodes.Status201Created).SetValues(entity)
			: new RequestResult<T>().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
	}

	/// <summary>Tries to modify an entity in the database with the specified uid.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="entity">The entity that will replace the current one.</param>
	/// <param name="id">The uid of the entity to replace.</param>
	/// <param name="setHiddenAttributes">If <c>true</c> even the attributes that are set to be hidden are set.</param>
	/// <typeparam name="T">The type of the entity.</typeparam>
	/// <returns>A <see cref="RequestResult{T}"/> containing the outcome of the operation.</returns>
	public static RequestResult<T> TryModifyEntity<T>(this LdapService ldap, T entity, string id, bool setHiddenAttributes = false) where T : class {
		if (!ldap.EntityExists<T>(id))
			return new RequestResult<T>().SetStatus(StatusCodes.Status404NotFound).SetErrors("The object does not exist.");

		var request = CreateModifyRequest(ldap, entity, id, setHiddenAttributes);

		return ldap.TryRequest(request, out string? error) is not null
			? new RequestResult<T>().SetStatus(StatusCodes.Status200OK).SetValues(entity)
			: new RequestResult<T>().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
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

		return ldap.TryRequest(request, out string? error) is not null
			? new RequestResult().SetStatus(StatusCodes.Status200OK)
			: new RequestResult().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
	}

	/// <summary>Tries to add the entities to the database.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="entities">The entities to be added.</param>
	/// <param name="idGetter">A delegate that takes in an entity of type <typeparamref name="T"/> and returns an uid with the type of <c>long</c>.</param>
	/// <param name="setHiddenAttributes">If <c>true</c> even the attributes that are set to be hidden are set.</param>
	/// <param name="overwrite">If <c>true</c> the existing entities will be overwritten.</param>
	/// <typeparam name="T">The type of the entities.</typeparam>
	/// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
	public static RequestResult TryAddEntities<T>(this LdapService ldap, IEnumerable<T> entities, Func<T, string> idGetter, bool setHiddenAttributes = false, bool overwrite = false) where T : class {
		var type = typeof(T);

		ldap.TryRequest(new AddRequest($"ou={type.GetOuName()},{ldap.DomainComponents}", "organizationalUnit"), false);

		DirectoryAttribute? objectClassesAttribute = null;
		if (type.GetCustomAttribute<LdapObjectClassesAttribute>() is { } objectClasses)
			objectClassesAttribute = new DirectoryAttribute("objectClass", objectClasses.Classes.Cast<object>().ToArray());

		var requests = entities.Select(entity => {
			string id = idGetter(entity);

			if (overwrite && ldap.EntityExists<T>(id))
				return new UniqueDirectoryRequest(CreateModifyRequest(ldap, entity, id, setHiddenAttributes), id);

			AddRequest request = new($"uid={id},ou={type.GetOuName()},{ldap.DomainComponents}");
			if (objectClassesAttribute is not null)
				request.Attributes.Add(objectClassesAttribute);

			foreach (var attribute in LdapService.GetDirectoryAttributes(entity, setHiddenAttributes))
				request.Attributes.Add(attribute);

			return new UniqueDirectoryRequest(request, id);
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
		if (string.IsNullOrWhiteSpace(id) || id == Authenticator.GetDefaultAdminName())
			return null;

		foreach (var type in typesToTry) {
			SearchRequest request  = new($"uid={id},ou={type.GetOuName()},{ldap.DomainComponents}", LdapService.AnyFilter, SearchScope.Base, "displayName");
			var           response = ldap.TryRequest(request) as SearchResponse;

			try {
				return response!.Entries[0].Attributes["displayName"][0].ToString();
			}
			catch {
				continue;
			}
		}

		return null;
	}

	private static ModifyRequest CreateModifyRequest<T>(LdapService ldap, T entity, string id, bool setHiddenAttributes) where T : class {
		var type = typeof(T);

		List<string> flags = [ ];

		IEnumerable<string> presentAttributes = ldap.TryGetPresentEntityAttributes<T>(id);

		ModifyRequest request = new($"uid={id},ou={type.GetOuName()},{ldap.DomainComponents}");

		foreach (var info in type.GetProperties())
			if (
				info.GetCustomAttribute(typeof(LdapAttributeAttribute)) is LdapAttributeAttribute attribute
				&& (!attribute.Hidden || setHiddenAttributes)
			) {
				if (info.GetValue(entity) is { } value) {
					DirectoryAttributeModification mod = new() {
						Name      = attribute.Name,
						Operation = DirectoryAttributeOperation.Replace
					};
					mod.Add(value.ToString());
					request.Modifications.Add(mod);
				}
				else if (presentAttributes.Contains(attribute.Name)) {
					request.Modifications.Add(new DirectoryAttributeModification {
						Name      = attribute.Name,
						Operation = DirectoryAttributeOperation.Delete
					});
				}
			}
			else if (
				info.GetCustomAttribute<LdapFlagAttribute>() is { } flag
				&& info.GetValue(entity) is true
			) {
				flags.Add(flag.Name);
			}


		if (flags.Count > 0) {
			DirectoryAttributeModification mod = new() {
				Name      = "description",
				Operation = DirectoryAttributeOperation.Replace
			};
			mod.Add(string.Join('|', flags));
			request.Modifications.Add(mod);
		}
		else if (presentAttributes.Contains("description")) {
			request.Modifications.Add(new DirectoryAttributeModification {
				Name      = "description",
				Operation = DirectoryAttributeOperation.Delete
			});
		}

		return request;
	}
}

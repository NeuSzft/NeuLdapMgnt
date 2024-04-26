using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api.LdapServiceExtensions;

public static class GroupExtensions {
	/// <summary>Checks if the group exists within the database using it's uid.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="name">The name of the group.</param>
	/// <returns><c>true</c> if the group exists. If it does not exist or the search request fails it returns <c>false</c>.</returns>
	public static bool GroupExists(this LdapService ldap, string name) {
		SearchRequest   request  = new($"ou={name},{ldap.DomainComponents}", LdapService.AnyFilter, SearchScope.Base, null);
		SearchResponse? response = ldap.TryRequest(request) as SearchResponse;

		return response?.Entries.Count == 1;
	}

	/// <summary>Checks if the entity with the uid is part of the group.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="name">The name of the group.</param>
	/// <param name="id">The uid of the entity.</param>
	/// <returns><c>true</c> if the entity is part of the group, <c>false</c> if not or the group does not exist.</returns>
	/// <remarks>This method does not check if the entity actually exists or not.</remarks>
	public static bool PartOfGroup(this LdapService ldap, string name, string id) {
		SearchRequest   request  = new($"ou={name},{ldap.DomainComponents}", LdapService.AnyFilter, SearchScope.Base, null);
		SearchResponse? response = ldap.TryRequest(request) as SearchResponse;

		if (response is null || response.Entries.Count == 0)
			return false;

		return response.Entries[0].Attributes["uid"].GetValues(typeof(string)).Any(x => x.ToString() == id);
	}

	/// <summary>Gets the uids of the members of the group.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="name">The name of the group.</param>
	/// <returns>An <see cref="IEnumerable{T}">IEnumerable&lt;string&gt;</see> containing the uids.</returns>
	/// <remarks>If the group does not exist it returns an empty collection.</remarks>
	public static IEnumerable<string> GetMembersOfGroup(this LdapService ldap, string name) {
		SearchRequest   request  = new($"ou={name},{ldap.DomainComponents}", LdapService.AnyFilter, SearchScope.Subtree, null);
		SearchResponse? response = ldap.TryRequest(request) as SearchResponse;

		if (response is null || response.Entries.Count == 0)
			return [];

		return response.Entries[0].Attributes["uid"].GetValues(typeof(string)).Select(x => x.ToString()).Where(x => x != "__DEFAULT__").NotNull();
	}

	/// <summary>Sets the group to contain the specified uids.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="name">The name of the group.</param>
	/// <param name="ids">The uids the group should contain.</param>
	/// <returns>A nullable string that will contain the error message if there was one.</returns>
	public static bool SetMembersOfGroup(this LdapService ldap, string name, IEnumerable<string> ids) {
		ldap.TryAddGroup(name);

		ModifyRequest request = new($"ou={name},{ldap.DomainComponents}");

		if (ids.Any()) {
			DirectoryAttributeModification mod = new() {
				Name = "uid",
				Operation = DirectoryAttributeOperation.Replace
			};
			mod.AddRange(ids.Prepend("__DEFAULT__").ToArray());
			request.Modifications.Add(mod);
		}

		return ldap.TryRequest(request) is not null;
	}

	/// <summary>Tries to add the group to the database with the specified name.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="name">The name of the group.</param>
	/// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
	public static RequestResult TryAddGroup(this LdapService ldap, string name) {
		if (ldap.GroupExists(name))
			return new RequestResult().SetStatus(StatusCodes.Status409Conflict).SetErrors("The group already exists.");

		AddRequest request = new($"ou={name},{ldap.DomainComponents}",
			new("objectClass", "organizationalUnit", "uidObject"),
			new("uid", "__DEFAULT__")
		);

		if (ldap.TryRequest(request, out var error) is not null)
			return new RequestResult().SetStatus(StatusCodes.Status201Created);
		return new RequestResult().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
	}

	/// <summary>Tries to remove an entity from the database with the specified uid.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="name">The name of the group.</param>
	/// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
	public static RequestResult TryDeleteGroup(this LdapService ldap, string name) {
		if (!ldap.GroupExists(name))
			return new RequestResult().SetStatus(StatusCodes.Status404NotFound).SetErrors("The group does not exist.");

		DeleteRequest request = new($"ou={name},{ldap.DomainComponents}");

		if (ldap.TryRequest(request, out var error) is not null)
			return new RequestResult().SetStatus(StatusCodes.Status200OK);
		return new RequestResult().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
	}

	/// <summary>Tries to add the entity to the group.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="name">The name of the group.</param>
	/// <param name="id">The uid of the entity to add.</param>
	/// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
	public static RequestResult TryAddEntityToGroup(this LdapService ldap, string name, string id) {
		ldap.TryAddGroup(name);

		if (ldap.PartOfGroup(name, id))
			return new RequestResult().SetStatus(StatusCodes.Status409Conflict).SetErrors("Already part of group.");

		ModifyRequest request = new($"ou={name},{ldap.DomainComponents}");

		DirectoryAttributeModification mod = new() {
			Name = "uid",
			Operation = DirectoryAttributeOperation.Add
		};
		mod.Add(id);
		request.Modifications.Add(mod);

		if (ldap.TryRequest(request, out var error) is not null)
			return new RequestResult().SetStatus(StatusCodes.Status200OK);
		return new RequestResult().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
	}

	/// <summary>Tries to add the entities to the group.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="name">The name of the group.</param>
	/// <param name="ids">The uids to be added.</param>
	/// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
	public static RequestResult TryAddEntitiesToGroup(this LdapService ldap, string name, IEnumerable<string> ids) {
		ldap.TryAddGroup(name);

		var requests = ids.Select(id => {
			ModifyRequest request = new($"ou={name},{ldap.DomainComponents}");

			DirectoryAttributeModification mod = new() {
				Name = "uid",
				Operation = DirectoryAttributeOperation.Add
			};
			mod.Add(id);
			request.Modifications.Add(mod);

			return new UniqueDirectoryRequest(request, id);
		});

		string[] errors = ldap.TryRequests(requests).Select(x => x.Error).NotNull().ToArray();
		return new RequestResult().SetStatus(StatusCodes.Status207MultiStatus).SetErrors(errors);
	}

	/// <summary>Tries to remove the entity from the group.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="name">The name of the group.</param>
	/// <param name="id">The uid of the entity to remove.</param>
	/// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
	public static RequestResult TryRemoveEntityFromGroup(this LdapService ldap, string name, string id) {
		if (!ldap.GroupExists(name))
			return new RequestResult().SetStatus(StatusCodes.Status404NotFound).SetErrors("The group does not exist.");

		if (!ldap.PartOfGroup(name, id))
			return new RequestResult().SetStatus(StatusCodes.Status404NotFound).SetErrors("Not part of group.");

		ModifyRequest request = new($"ou={name},{ldap.DomainComponents}");

		DirectoryAttributeModification mod = new() {
			Name = "uid",
			Operation = DirectoryAttributeOperation.Delete
		};
		mod.Add(id);
		request.Modifications.Add(mod);

		if (ldap.TryRequest(request, out var error) is not null)
			return new RequestResult().SetStatus(StatusCodes.Status200OK);
		return new RequestResult().SetStatus(StatusCodes.Status400BadRequest).SetErrors(error ?? string.Empty);
	}

	/// <summary>Tries to remove the entities from the group.</summary>
	/// <param name="ldap">The <see cref="LdapService"/> the method should use.</param>
	/// <param name="name">The name of the group.</param>
	/// <param name="ids">The uids to be removed.</param>
	/// <returns>A <see cref="RequestResult"/> containing the outcome of the operation.</returns>
	public static RequestResult TryRemoveEntitiesFromGroup(this LdapService ldap, string name, IEnumerable<string> ids) {
		if (!ldap.GroupExists(name))
			return new RequestResult().SetStatus(StatusCodes.Status404NotFound).SetErrors("The group does not exist.");

		var requests = ids.Select(id => {
			ModifyRequest request = new($"ou={name},{ldap.DomainComponents}");

			DirectoryAttributeModification mod = new() {
				Name = "uid",
				Operation = DirectoryAttributeOperation.Delete
			};
			mod.Add(id);
			request.Modifications.Add(mod);

			return new UniqueDirectoryRequest(request, id);
		});

		string[] errors = ldap.TryRequests(requests).Select(x => x.Error).NotNull().ToArray();
		return new RequestResult().SetStatus(StatusCodes.Status207MultiStatus).SetErrors(errors);
	}
}

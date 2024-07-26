using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using NeuLdapMgnt.Models;
using PluralizeService.Core;

namespace NeuLdapMgnt.Api;

public static class Utils {
	/// <summary>Returns a new <see cref="SymmetricSecurityKey"/> that is either loaded from the specified environment variable as a base64 string or creates a new one with the specified size.</summary>
	/// <param name="env">Name of the environment variable.</param>
	/// <param name="size">The size of the key in bytes to be generated if the env is not found.</param>
	/// <returns>The new <see cref="SymmetricSecurityKey"/>.</returns>
	public static SymmetricSecurityKey LoadOrCreateSecurityKey(string env, int size = 256) {
		string? base64Key = Environment.GetEnvironmentVariable(env);
		return new(base64Key is not null ? Convert.FromBase64String(base64Key) : RandomNumberGenerator.GetBytes(size));
	}

	/// <summary>Hashes the password using bcrypt.</summary>
	/// <param name="password">The password to hash.</param>
	/// <returns>The hashed password with the <c>"{CRYPT}"</c> string prepended to it.</returns>
	public static string BCryptHashPassword(string password) {
		return "{CRYPT}" + BCrypt.Net.BCrypt.HashPassword(password);
	}

	/// <summary>Checks if the password matches the bcrypt hash.</summary>
	/// <param name="hash">The bcrypt hash.</param>
	/// <param name="password">The password to check.</param>
	/// <returns><c>true</c> if the password matches the provided hash, otherwise <c>false</c>.</returns>
	public static bool CheckBCryptPassword(string hash, string password) {
		try {
			return BCrypt.Net.BCrypt.Verify(password, hash.Replace("{CRYPT}", null));
		}
		catch {
			return false;
		}
	}

	/// <summary>Gets the version of an assembly in the <c>&lt;major&gt;.&lt;minor&gt;.&lt;build&gt;</c> format.</summary>
	/// <param name="assembly">The <see cref="Assembly"/> the use.</param>
	/// <returns>The version of the assembly or <c>"unspecified"</c> if it is not specified.</returns>
	public static string GetAssemblyVersion(Assembly assembly) {
		return assembly.GetName().Version is { } v ? $"{v.Major}.{v.Minor}.{v.Build}" : "unspecified";
	}

	/// <summary>Tries to parse the csv line and create either a <see cref="Student"/> or an <see cref="Employee"/> from it.</summary>
	/// <param name="csv">The csv string to parse.</param>
	/// <returns>A <see cref="Person"/> if the parsing was successful or <c>null</c> if it was not.</returns>
	public static Person? GetPersonFromCsv(string csv) {
		// TODO: Implement csv parsing
		throw new NotImplementedException();
	}

	/// <summary>Tries to parse each csv line and create either a <see cref="Student"/> or an <see cref="Employee"/> from it.</summary>
	/// <param name="csvLines">The lines of csv to parse.</param>
	/// <returns>All successfully parsed <see cref="Person"/> objects.</returns>
	public static IEnumerable<Person> GetPersonsFromCsv(IEnumerable<string> csvLines) {
		foreach (string csv in csvLines)
			if (GetPersonFromCsv(csv) is { } person)
				yield return person;
	}

	/// <summary>Gets the value of an environment variable.</summary>
	/// <param name="name">The name of the env.</param>
	/// <returns>The value of the env.</returns>
	/// <exception cref="ApplicationException">The value of the env is <c>null</c> or whitespace.</exception>
	public static string GetEnv(string name) {
		string? env = Environment.GetEnvironmentVariable(name);
		if (string.IsNullOrWhiteSpace(env))
			throw new ApplicationException($"The '{name}' environment variable is not defined");
		return env;
	}

	/// <summary>Gets the value of an environment variable.</summary>
	/// <param name="name">The name of the env.</param>
	/// <param name="fallback">The value to return if the env is unspecified.</param>
	/// <returns>The value of the env or the <paramref name="fallback"/> value if it is <c>null</c> or whitespace.</returns>
	public static string GetEnv(string name, string fallback) {
		return Environment.GetEnvironmentVariable(name).FallbackIfNullOrWhitespace(fallback);
	}

	/// <summary>Gets the value of an environment variable and determines if its value is <c>true</c> or not.</summary>
	/// <param name="name">The name of the env.</param>
	/// <returns><c>true</c> if the value of the env can be parsed into a valid <see cref="bool"/> and it is also <c>true</c>.</returns>
	public static bool IsEnvTrue(string name) {
		return bool.TryParse(Environment.GetEnvironmentVariable(name), out bool value) && value;
	}
}

public static class ExtensionUtils {
	/// <summary>If the string is <c>null</c> or it only contains whitespaces then it returns <paramref name="fallback"/>, otherwise it returns the original string.</summary>
	/// <param name="str">The original string.</param>
	/// <param name="fallback">The string to return if the original string is null or empty.</param>
	/// <returns>The original string or <paramref name="fallback"/>.</returns>
	public static string FallbackIfNullOrWhitespace(this string? str, string fallback) {
		return string.IsNullOrWhiteSpace(str) ? fallback : str;
	}

	/// <summary>Returns the elements of the collection that are not <c>null</c>.</summary>
	/// <param name="collection">The collection of elements.</param>
	/// <typeparam name="T">The type of the elements.</typeparam>
	/// <returns>The elements that are not <c>null</c>.</returns>
	public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> collection) {
		return collection.Where(x => x is not null).Cast<T>();
	}

	/// <summary>Gets the error of an exception.</summary>
	/// <param name="e">The exception to get the error from.</param>
	/// <returns>The message of the exception, or if that is null or empty, the longest available name of the exception's type.</returns>
	public static string GetError(this Exception e) {
		Type type = e.GetType();
		return FallbackIfNullOrWhitespace(e.Message, type.FullName ?? type.Name);
	}

	/// <summary>Get the of a type that should be used for its organizational unit.</summary>
	/// <param name="type">The type to get the name of.</param>
	/// <returns>The lowercase pluralized form of <paramref name="type"/>'s <c>Name</c>.</returns>
	public static string GetOuName(this Type type) {
		return PluralizationProvider.Pluralize(type.Name.ToLower());
	}

	/// <summary>Sets the <c>NewToken</c> of result to a new JSON Web Token.</summary>
	/// <param name="result">The <see cref="RequestResult"/> to add the new token to.</param>
	/// <param name="request">The <see cref="HttpRequest"/> to get the Authorization header from, containing the current token.</param>
	/// <typeparam name="T">The type of the result which must inherit from <see cref="RequestResult"/>.</typeparam>
	/// <returns>The <paramref name="result"/> with its <c>NewToken</c> set.</returns>
	public static T RenewToken<T>(this T result, HttpRequest request) where T : RequestResult {
		result.SetToken(Authenticator.RenewToken(request));
		return result;
	}

	/// <summary>Returns an <see cref="IResult"/> that contains the <see cref="RequestResult"/> serialized to json and its status code set to the result's <c>StatusCode</c>.</summary>
	/// <param name="result">The <see cref="RequestResult"/> to serialize to json.</param>
	/// <returns>An <see cref="IResult"/> containing the <see cref="RequestResult"/> serilaized to json.</returns>
	public static IResult ToResult(this RequestResult result) {
		return Results.Content(JsonSerializer.Serialize(result, result.GetType()), MediaTypeNames.Application.Json, Encoding.UTF8, result.StatusCode);
	}

	/// <summary>Tries to get the address of the client that sent the request.</summary>
	/// <param name="context">The <see cref="HttpContext"/> of the request.</param>
	/// <param name="checkHeaders">If <c>true</c> the method should try to get the address of the remote from the "X-Real-IP" or "X-Forwarded-For" headers. If <c>false</c> or the headers do not specify an address it will return the remote address of the <see cref="ConnectionInfo"/>.</param>
	/// <returns>The IP address of the client as a string or <c>null</c> if it cannot be determined.</returns>
	public static string? TryGetClientAddress(this HttpContext context, bool checkHeaders) {
		if (checkHeaders) {
			if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
				return realIp;
			if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var ff) && ff.FirstOrDefault()?.Split(',').Last().Trim() is { } ffIp)
				return ffIp;
		}
		return context.Connection.RemoteIpAddress?.ToString();
	}

	/// <summary>Sets the password of a <see cref="Person"/>.</summary>
	/// <param name="person">The <see cref="Person"/> to set the password of.</param>
	/// <param name="password">The plain text password to be hashed.</param>
	public static void SetPassword(this Person person, string password) {
		person.Password = Utils.BCryptHashPassword(password);
	}

	/// <summary>Checks if the password's hash is the same as the <see cref="Person"/>'s.</summary>
	/// <param name="person">The <see cref="Person"/> to check the password hash of.</param>
	/// <param name="password">The password to hash and check.</param>
	/// <returns><c>true</c> if the hashes match, otherwise <c>false</c>.</returns>
	public static bool CheckPassword(this Person person, string password) {
		return person.Password is not null && Utils.CheckBCryptPassword(person.Password, password);
	}
}

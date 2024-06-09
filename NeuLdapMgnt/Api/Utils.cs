using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
		return new(base64Key is null ? RandomNumberGenerator.GetBytes(size) : Convert.FromBase64String(base64Key));
	}

	/// <summary>Hashes the password using bcrypt.</summary>
	/// <param name="password">The password to hash.</param>
	/// <returns>The hashed password with the "<c>{CRYPT}</c>" string prepended to it.</returns>
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

	/// <summary>Gets the version of the assembly in the <c>major.minor.revision</c> format.</summary>
	/// <returns>The version of the assembly or 1.0.0 if it is not specified.</returns>
	public static string GetAssemblyVersion(Assembly assembly) {
		return assembly.GetName().Version is { } v ? $"{v.Major}.{v.Minor}.{v.Revision}" : "1.0.0";
	}
}

public static class ExtensionUtils {
	/// <summary>If the provided string is <c>null</c> or has the length of zero then it returns <paramref name="defaultStr"/>, otherwise it returns the original string.</summary>
	/// <param name="str">The original string.</param>
	/// <param name="defaultStr">The string to return if the the original string is null or empty.</param>
	/// <returns>The original string or <paramref name="defaultStr"/>.</returns>
	public static string DefaultIfNullOrEmpty(this string? str, string defaultStr) {
		if (str is null || str.Length == 0)
			return defaultStr;
		return str;
	}

	/// <summary>If the provided string is <c>null</c> or has the length of zero then an exception is thrown.</summary>
	/// <param name="str">The string to check.</param>
	/// <param name="nameOf">The name of the string variable.</param>
	/// <returns>The original string.</returns>
	/// <exception cref="ArgumentException">The provided string is <c>null</c> or has the length of zero.</exception>
	public static string ThrowIfNullOrEmpty(this string? str, string nameOf) {
		if (str is null || str.Length == 0)
			throw new ArgumentException($"{nameOf} is null or empty");
		return str;
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
		return e.Message.DefaultIfNullOrEmpty(type.FullName ?? type.Name);
	}

	/// <summary>Get the the of a type that should be used for it's organizational unit.</summary>
	/// <param name="type">The type to get the name of.</param>
	/// <returns>The lowercase pluralized form of <paramref name="type"/>'s <c>Name</c>.</returns>
	public static string GetOuName(this Type type) {
		return PluralizationProvider.Pluralize(type.Name.ToLower());
	}

	/// <summary>Sets the <c>NewToken</c> of result to a new JSON Web Token.</summary>
	/// <param name="result">The <see cref="RequestResult"/> to add the new token to.</param>
	/// <param name="request">The <see cref="HttpRequest"/> to get the Authorization header from, containing the current token.</param>
	/// <typeparam name="T">The type of the result which must inherit from <see cref="RequestResult"/>.</typeparam>
	/// <returns>The <paramref name="result"/> with it's <c>NewToken</c> set.</returns>
	public static T RenewToken<T>(this T result, HttpRequest request) where T : RequestResult {
		result.SetToken(Authenticator.RenewToken(request));
		return result;
	}

	/// <summary>Returns an <see cref="IResult"/> that contains the <see cref="RequestResult"/> serialized to json and it's status code set to the result's <c>StatusCode</c>.</summary>
	/// <param name="result">The <see cref="RequestResult"/> to serialize to json.</param>
	/// <returns>An <see cref="IResult"/> containing the <see cref="RequestResult"/> serilaized to json.</returns>
	public static IResult ToResult(this RequestResult result) {
		return Results.Content(JsonSerializer.Serialize(result, result.GetType()), "application/json", Encoding.UTF8, result.StatusCode);
	}

	/// <summary>Tries to get the address of the client that sent the request.</summary>
	/// <param name="context">The <see cref="HttpContext"/> of the request.</param>
	/// <returns>The IP address of the client as a string or <c>null</c>.</returns>
	public static string? TryGetClientAddress(this HttpContext context) {
		if (context.Request.Headers.TryGetValue("X-Real-IP", out var ip))
			return ip;
		if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
			return forwarded.FirstOrDefault();
		return context.Connection.RemoteIpAddress?.ToString();
	}

	public static void SetPassword(this Person person, string password) {
		person.Password = Utils.BCryptHashPassword(password);
	}

	public static bool CheckPassword(this Person person, string password) {
		if (person.Password is null)
			return false;
		return Utils.CheckBCryptPassword(person.Password, password);
	}
}

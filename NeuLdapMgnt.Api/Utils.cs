using System;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace NeuLdapMgnt.Api;

internal static class Utils {
    /// <summary>Returns a new <see cref="SymmetricSecurityKey"/> that is either loaded from the specified env as a base64 string or creates a new one with the specified size.</summary> 
    /// <param name="env">Name of the environment variable.</param>
    /// <param name="size">The size of the key in bytes to be generated if the env is not found.</param>
    /// <returns>The new <see cref="SymmetricSecurityKey"/>.</returns>
    public static SymmetricSecurityKey LoadOrCreateSecurityKey(string env, int size = 256) {
        string? base64Key = Environment.GetEnvironmentVariable(env);
        return new(base64Key is null ? RandomNumberGenerator.GetBytes(size) : Convert.FromBase64String(base64Key));
    }

    /// Returns <paramref name="defaultStr"/> if the string is null or has a length of zero.
    public static string DefaultIfNullOrEmpty(this string? str, string defaultStr) {
        if (str is null || str.Length == 0)
            return defaultStr;
        return str;
    }
}

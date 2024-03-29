using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using NeuLdapMgnt.Models;
using PluralizeService.Core;

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

    public static async Task<(List<Student> Students, string? Error)> CsvToStudents(StreamReader reader, int startUid = 6000) {
        List<Student> students = new();
        int           uid      = startUid;
        int           lineNum  = 0;

        while (await reader.ReadLineAsync() is { } line) {
            lineNum++;
            if (line.Length == 0)
                continue;

            Student student = new();
            students.Add(student);

            string[] arr = line.Split([',', ';']);

            try {
                long.TryParse(arr[0], out var id);

                student.Id            = id;
                student.Uid           = uid;
                student.Gid           = uid;
                student.FirstName     = arr[1].Trim('"');
                student.LastName      = arr[2].Trim('"');
                student.Username      = student.GetUsername();
                student.Class         = "12.A";
                student.Email         = arr[3].Trim('"');
                student.Password      = arr[4].Trim('"');
                student.HomeDirectory = $"/home/{student.Username}";

                var errors = ModelValidator.Validate(student).Errors;
                if (errors.Any())
                    return ([], $"Error on line {lineNum}:\n{string.Join('\n', errors)}");
            }
            catch {
                return ([], $"Error on line {lineNum}");
            }

            uid++;
        }

        return (students, null);
    }
}

internal static class ExtensionsUtils {
    /// Returns <paramref name="defaultStr"/> if the string is null or has a length of zero.
    public static string DefaultIfNullOrEmpty(this string? str, string defaultStr) {
        if (str is null || str.Length == 0)
            return defaultStr;
        return str;
    }

    public static string ThrowIfNullOrEmpty(this string? str, string? nameOf = null) {
        if (str is null || str.Length == 0)
            throw new ArgumentException($"{nameOf ?? "string"} is null or empty");
        return str;
    }

    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> collection) {
        return collection.Where(x => x is not null).Cast<T>();
    }

    public static string GetError(this Exception e) {
        Type type = e.GetType();
        return e.Message.DefaultIfNullOrEmpty(type.FullName ?? type.Name);
    }

    public static string GetOuName(this Type type) {
        return PluralizationProvider.Pluralize(type.Name.ToLower());
    }
}

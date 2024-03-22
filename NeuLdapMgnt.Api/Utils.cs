using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using NeuLdapMgnt.Models;
using PluralizeService.Core;
using RuntimeHelpers = System.Runtime.CompilerServices.RuntimeHelpers;

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

    public static string GetError(this Exception e) {
        Type type = e.GetType();
        return e.Message.DefaultIfNullOrEmpty(type.FullName ?? type.Name);
    }

    public static string GetOuName(this Type type) {
        return PluralizationProvider.Pluralize(type.Name.ToLower());
    }

    public static T ForceCreateClassObj<T>() where T : class {
        Type             type = typeof(T);
        ConstructorInfo? ctor = type.GetConstructor(Type.EmptyTypes);
        return ctor?.Invoke(null) as T ?? (T)RuntimeHelpers.GetUninitializedObject(type);
    }

    public static async Task<List<Student>> CsvToStudents(StreamReader reader, int startUid = 6000) {
        List<Student> students = new();

        while (await reader.ReadLineAsync() is { } line) {
            if (line.Length == 0)
                continue;

            Student student = ForceCreateClassObj<Student>();
            students.Add(student);

            string[] arr = line.Split(',');
            student.Id        = arr[0];
            student.Uid       = startUid;
            student.Gid       = startUid;
            student.FirstName = arr[1].Trim('"');
            student.LastName  = arr[2].Trim('"');
            student.Class     = $"{startUid % 5 + 8}.A";
            student.Email     = arr[3].Trim('"');
            student.Password  = arr[4].Trim('"');

            startUid++;
        }

        return students;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.Api;

/// <summary>A helper for validating models with <see cref="ValidationAttribute"/>s.</summary>
public static class ModelValidator {
    /// <summary>Validates the <see cref="ValidationAttribute"/>s of a model.</summary>
    /// <param name="obj">The object to validate.</param>
    /// <typeparam name="T">The type of the model.</typeparam>
    /// <returns>A <see cref="RequestResult{T}"/> containing the result of the validation.</returns>
    public static RequestResult<T> Validate<T>(T obj) where T : class {
        List<ValidationResult> results = new();
        if (Validator.TryValidateObject(obj, new ValidationContext(obj), results, true))
            return new RequestResult<T>().SetStatus(StatusCodes.Status201Created).SetValues(obj);
        return new RequestResult<T>().SetStatus(StatusCodes.Status400BadRequest).SetErrors(results.Select(x => x.ErrorMessage).NotNull().ToArray());
    }

    /// <summary>Tries to deserializes the json string, then validate the <see cref="ValidationAttribute"/>s of the model.</summary>
    /// <param name="json">The json string to deserialize.</param>
    /// <typeparam name="T">The type of the model.</typeparam>
    /// <returns>A <see cref="RequestResult{T}"/> containing the result of the validation.</returns>
    public static RequestResult<T> ValidateJson<T>(string json) where T : class {
        T? obj;
        try {
            obj = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { AllowTrailingCommas = true })!;
        }
        catch (Exception e) {
            return new RequestResult<T>().SetStatus(StatusCodes.Status400BadRequest).SetErrors(e.GetError());
        }

        return Validate(obj);
    }

    /// <summary>Tries to deserializes the json body of the <see cref="HttpRequest"/>, then validate the <see cref="ValidationAttribute"/>s of the model.</summary>
    /// <param name="request">The request containing the json to deserialize within it's body.</param>
    /// <typeparam name="T">The type of the model.</typeparam>
    /// <returns>A <see cref="RequestResult{T}"/> containing the result of the validation.</returns>
    public static async Task<RequestResult<T>> ValidateRequest<T>(HttpRequest request) where T : class {
        using StreamReader reader = new(request.Body);
        string             json   = await reader.ReadToEndAsync();
        return ValidateJson<T>(json);
    }
}

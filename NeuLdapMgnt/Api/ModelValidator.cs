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

public static class ModelValidator {
    public static RequestResult<T> Validate<T>(T obj) where T : class {
        List<ValidationResult> results = new();
        if (Validator.TryValidateObject(obj, new ValidationContext(obj), results, true))
            return new RequestResult<T>().SetStatus(StatusCodes.Status201Created).SetValues(obj);
        return new RequestResult<T>().SetStatus(StatusCodes.Status400BadRequest).SetErrors(results.Select(x => x.ErrorMessage).NotNull().ToArray());
    }

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

    public static async Task<RequestResult<T>> ValidateRequest<T>(HttpRequest request) where T : class {
        using StreamReader reader = new(request.Body);
        string             json   = await reader.ReadToEndAsync();
        return ValidateJson<T>(json);
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NeuLdapMgnt.Api;

public record ModelValidationResult<T>(T? Result, IEnumerable<string> Errors) {
    public IResult ToResult(int defaultSuccessCode = StatusCodes.Status201Created) {
        bool success = Result is not null;
        int  code    = success ? defaultSuccessCode : StatusCodes.Status400BadRequest;
        return Results.Json(new { success, errors = Errors }, new JsonSerializerOptions { WriteIndented = true }, "application/json", code);
    }
}

public static class ModelValidator {
    public static ModelValidationResult<T> Validate<T>(T obj) where T : class {
        List<ValidationResult> results = new();
        bool                   success = Validator.TryValidateObject(obj, new ValidationContext(obj), results, true);
        return new(success ? obj : null, results.Select(x => x.ErrorMessage).NotNull());
    }

    public static ModelValidationResult<T> ValidateJson<T>(string json) where T : class {
        T? obj;
        try {
            obj = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { AllowTrailingCommas = true })!;
        }
        catch (JsonException e) {
            return new(null, [e.GetError()]);
        }
        catch (Exception e) {
            return new(null, [e.ToString()]);
        }

        return Validate(obj);
    }

    public static async Task<ModelValidationResult<T>> ValidateRequest<T>(HttpRequest request) where T : class {
        using StreamReader reader = new(request.Body);
        string             json   = await reader.ReadToEndAsync();
        return ValidateJson<T>(json);
    }
}

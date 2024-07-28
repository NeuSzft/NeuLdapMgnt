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
	/// <summary>Tries to validate the <paramref name="value"/> against the validation <paramref name="attributes"/>.</summary>
	/// <param name="value">The value to validate.</param>
	/// <param name="attributes">The <see cref="ValidationAttribute"/>s to use.</param>
	/// <typeparam name="T">The type of the model.</typeparam>
	/// <returns>A <see cref="RequestResult{T}"/> containing the result of the validation.</returns>
	public static RequestResult ValidateValue<T>(T value, params ValidationAttribute[] attributes) where T : class {
		List<ValidationResult> results = new();
		if (Validator.TryValidateValue(value, new ValidationContext(new { value }), results, attributes))
			return new RequestResult().SetStatus(StatusCodes.Status201Created);
		return new RequestResult().SetStatus(StatusCodes.Status400BadRequest).SetErrors(results.Select(x => x.ErrorMessage).NotNull().ToArray());
	}

	/// <summary>Validates the <see cref="ValidationAttribute"/>s of a model.</summary>
	/// <param name="obj">The object to validate.</param>
	/// <typeparam name="T">The type of the model.</typeparam>
	/// <returns>A <see cref="RequestResult"/> containing the result of the validation.</returns>
	public static RequestResult Validate<T>(T obj) where T : class {
		List<ValidationResult> results = new();
		if (Validator.TryValidateObject(obj, new ValidationContext(obj), results, true))
			return new RequestResult().SetStatus(StatusCodes.Status201Created);
		return new RequestResult().SetStatus(StatusCodes.Status400BadRequest).SetErrors(results.Select(x => x.ErrorMessage).NotNull().ToArray());
	}

	/// <summary>Tries to deserializes the json string, then validate the <see cref="ValidationAttribute"/>s of the model.</summary>
	/// <param name="json">The json string to deserialize.</param>
	/// <typeparam name="T">The type of the model.</typeparam>
	/// <returns>A <see cref="RequestResult{T}"/> containing the result of the validation.</returns>
	public static RequestResult<T> ValidateJson<T>(string json) where T : class {
		try {
			var obj = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { AllowTrailingCommas = true })!;
			return Validate(obj).ToGeneric(obj);
		}
		catch (Exception e) {
			return new RequestResult<T>().SetStatus(StatusCodes.Status400BadRequest).SetErrors(e.GetError());
		}
	}

	/// <summary>Tries to deserializes the json body of the <see cref="HttpRequest"/>, then validate the <see cref="ValidationAttribute"/>s of the model.</summary>
	/// <param name="request">The request containing the json to deserialize within its body.</param>
	/// <typeparam name="T">The type of the model.</typeparam>
	/// <returns>A <see cref="RequestResult{T}"/> containing the result of the validation.</returns>
	public static async Task<RequestResult<T>> ValidateRequest<T>(HttpRequest request) where T : class {
		using StreamReader reader = new(request.Body);
		string             json   = await reader.ReadToEndAsync();
		return ValidateJson<T>(json);
	}
}

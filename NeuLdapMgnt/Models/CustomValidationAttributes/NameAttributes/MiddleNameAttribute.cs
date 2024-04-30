using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models.CustomValidationAttributes.NameAttributes;

public class MiddleNameAttribute : ValidationAttribute {
	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
		return value switch {
			string { Length: >= 3 } or null => ValidationResult.Success,
			string                          => new ValidationResult("Middle name must be at least 3 characters long.", new[] { validationContext.MemberName! }),
			_                               => new ValidationResult("Middle name: Invalid data type", new[] { validationContext.MemberName! })
		};
	}
}

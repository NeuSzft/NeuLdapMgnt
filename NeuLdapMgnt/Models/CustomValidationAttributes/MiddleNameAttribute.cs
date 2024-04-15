using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models.CustomValidationAttributes
{
	public class MiddleNameAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is string middleName)
			{
				if (string.IsNullOrEmpty(middleName) || string.IsNullOrWhiteSpace(middleName))
				{
					return ValidationResult.Success;
				}
				else if (middleName.Length < 3)
				{
					return new ValidationResult("Middle name must be at least 3 characters long.",
						new[] { validationContext.MemberName }!);
				}
				else
				{
					return ValidationResult.Success;
				}
			}

			return new ValidationResult("Middle name: Invalid data type", new[] { validationContext.MemberName }!);
		}
	}
}

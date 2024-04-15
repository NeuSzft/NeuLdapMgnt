using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models.CustomValidationAttributes
{
	public class LastNameAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is string lastName)
			{
				if (lastName.Length < 3)
				{
					return new ValidationResult("Last name must be at least 3 characters long.",
						new[] { validationContext.MemberName }!);
				}
				else
				{
					return ValidationResult.Success;
				}
			}

			return new ValidationResult("Last name: Invalid data type", new[] { validationContext.MemberName }!);
		}
	}
}

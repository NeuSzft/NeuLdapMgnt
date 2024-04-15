using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models.CustomValidationAttributes.AdminAttributes
{
	public class AdminIdAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is string id)
			{
				if (id.Length < 3)
				{
					return new ValidationResult("ID must be at least 3 characters long.",
						new[] { validationContext.MemberName }!);
				}
				else
				{
					return ValidationResult.Success;
				}
			}

			return new ValidationResult("ID: Invalid data type", new[] { validationContext.MemberName }!);
		}
	}
}

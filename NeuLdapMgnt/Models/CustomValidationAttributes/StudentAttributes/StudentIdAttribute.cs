using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models.CustomValidationAttributes.StudentAttributes
{
	public class StudentIdAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is long id)
			{
				if (id is < 70000000000 or > 79999999999)
				{
					return new ValidationResult($"OM must be between {70000000000} and {79999999999}.",
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

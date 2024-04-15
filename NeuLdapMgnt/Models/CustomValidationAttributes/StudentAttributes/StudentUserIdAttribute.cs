using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models.CustomValidationAttributes.StudentAttributes
{
	public class StudentUserIdAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is int uid)
			{
				if (uid is < 6000 or > 6999)
				{
					return new ValidationResult($"User ID must be between {6000} and {6999}.",
						new[] { validationContext.MemberName }!);
				}
				else
				{
					return ValidationResult.Success;
				}
			}

			return new ValidationResult("User ID: Invalid data type", new[] { validationContext.MemberName }!);
		}
	}
}

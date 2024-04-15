using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models.CustomValidationAttributes.AdminAttributes
{
	public class AdminUserIdAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is int uid)
			{
				if (uid is < 1000 or > 1999)
				{
					return new ValidationResult($"User ID must be between {1000} and {1999}.",
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

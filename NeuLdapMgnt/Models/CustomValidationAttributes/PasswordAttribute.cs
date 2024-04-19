using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NeuLdapMgnt.Models.CustomValidationAttributes
{
	public class PasswordAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is null)
			{
				return ValidationResult.Success;
			}

			if (value is string password)
			{
				if(string.IsNullOrEmpty(password))
				{
					return ValidationResult.Success;
				}
				if (password.Length < 8)
				{
					return new ValidationResult("Password must be at least 8 characters long.",
						new[] { validationContext.MemberName }!);
				}
				else if (!password.Any(char.IsLower))
				{
					return new ValidationResult("Password must contain at least one lowercase letter.",
						new[] { validationContext.MemberName }!);
				}
				else if (!password.Any(char.IsUpper))
				{
					return new ValidationResult("Password must contain at least one uppercase letter.",
						new[] { validationContext.MemberName }!);
				}
				else if (!password.Any(char.IsDigit))
				{
					return new ValidationResult("Password must contain at least one number.",
						new[] { validationContext.MemberName }!);
				}
				else if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
				{
					return new ValidationResult("Password must contain at least one special character.",
						new[] { validationContext.MemberName }!);
				}
				else
				{
					return ValidationResult.Success;
				}
			}

			return new ValidationResult("Password: Invalid data type", new[] { validationContext.MemberName }!);
		}
	}
}

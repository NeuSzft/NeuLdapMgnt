using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models.CustomValidationAttributes
{
	public class EmailAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value == null)
			{
				return ValidationResult.Success;
			}

			if (value is string email)
			{
				if (string.IsNullOrEmpty(email))
				{
					return ValidationResult.Success;
				}
				if (!email.Contains('@') || email.EndsWith('@'))
				{
					return new ValidationResult("Email is not a valid email address.",
						new[] { validationContext.MemberName }!);
				}
				if (email.Contains('@') && email.Count(x => x.Equals('@')) > 1)
				{
					return new ValidationResult("Email is not a valid email address.",
						new[] { validationContext.MemberName }!);
				}
				if (!email.All(x => char.IsLetterOrDigit(x) || x == '.' || x== '@'))
				{
					return new ValidationResult("Email must contain only alphanumeric characters '.', '@'.",
						new[] { validationContext.MemberName }!);
				}
				if (email.Contains('@') && !(email.StartsWith('@') || email.EndsWith('@')))
				{
					return ValidationResult.Success;
				}
			}

			return new ValidationResult("Email: Invalid data type", new[] { validationContext.MemberName }!);
		}
	}
}

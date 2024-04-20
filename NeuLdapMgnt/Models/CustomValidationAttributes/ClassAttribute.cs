using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models.CustomValidationAttributes
{
	public class ClassAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is null)
			{
				return ValidationResult.Success;
			}

			if (value is string @class)
			{
				if (string.IsNullOrEmpty(@class))
				{
					return ValidationResult.Success;
				}
				else if (@class.EndsWith(".", StringComparison.OrdinalIgnoreCase))
				{
					return new ValidationResult("Group is required.",
						new[] { validationContext.MemberName }!);
				}
				else if (!@class.Any(char.IsDigit))
				{
					return new ValidationResult("Year is required.",
						new[] { validationContext.MemberName }!);
				}
				else if (@class.EndsWith("ny", StringComparison.OrdinalIgnoreCase)
					&& !@class.StartsWith("9", StringComparison.OrdinalIgnoreCase))
				{
					return new ValidationResult("Only year '9' can be selected for 'Ny' group.",
						new[] { validationContext.MemberName }!);
				}
				else if (@class.EndsWith("rsze", StringComparison.OrdinalIgnoreCase)
					&& !@class.Contains('/', StringComparison.OrdinalIgnoreCase))
				{
					return new ValidationResult("Only year '1/13' and '2/14' can be selected for 'RSZE' group.",
						new[] { validationContext.MemberName }!);
				}
				else if (@class.Contains('/', StringComparison.OrdinalIgnoreCase)
					&& !(@class.Contains('a', StringComparison.OrdinalIgnoreCase)
					|| @class.Contains('b', StringComparison.OrdinalIgnoreCase)))
				{
					return new ValidationResult("Only group 'A', 'A.RSZE' or 'B', 'B.RSZE' can be selected.",
						new[] { validationContext.MemberName }!);
				}
				else
				{
					return ValidationResult.Success;
				}
			}

			return new ValidationResult("Class: Invalid data type", new[] { validationContext.MemberName }!);
		}
	}
}

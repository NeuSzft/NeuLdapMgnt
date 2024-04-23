using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models.CustomValidationAttributes
{
	public class ClassAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is string cls)
			{
				if (cls.Equals("-", StringComparison.OrdinalIgnoreCase))
				{
					return ValidationResult.Success;
				}

				if (cls.EndsWith(".", StringComparison.OrdinalIgnoreCase))
				{
					return new ValidationResult("Group is required.",
						new[] { validationContext.MemberName }!);
				}

				if (!cls.Any(char.IsDigit))
				{
					return new ValidationResult("Year is required.",
						new[] { validationContext.MemberName }!);
				}

				if (cls.EndsWith("ny", StringComparison.OrdinalIgnoreCase)
					&& !cls.StartsWith("9", StringComparison.OrdinalIgnoreCase))
				{
					return new ValidationResult("Only year '9' can be selected for 'Ny' group.",
						new[] { validationContext.MemberName }!);
				}

				if (cls.EndsWith("rsze", StringComparison.OrdinalIgnoreCase)
					&& !cls.Contains('/', StringComparison.OrdinalIgnoreCase))
				{
					return new ValidationResult("Only year '1/13' and '2/14' can be selected for 'RSZE' group.",
						new[] { validationContext.MemberName }!);
				}

				if (cls.Contains('/', StringComparison.OrdinalIgnoreCase)
					&& !(cls.Contains('a', StringComparison.OrdinalIgnoreCase)
					|| cls.Contains('b', StringComparison.OrdinalIgnoreCase)))
				{
					return new ValidationResult("Only group 'A', 'A.RSZE' or 'B', 'B.RSZE' can be selected.",
						new[] { validationContext.MemberName }!);
				}

				return ValidationResult.Success;
			}

			return new ValidationResult("Class: Invalid data type", new[] { validationContext.MemberName }!);
		}
	}
}

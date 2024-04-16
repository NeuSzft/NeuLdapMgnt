using System;
using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models.CustomValidationAttributes
{
	public class DirectoryAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is string directory)
			{
				directory = directory.Trim();
				if (!directory.StartsWith("/home/", StringComparison.OrdinalIgnoreCase))
				{
					return new ValidationResult("Directory must start with '/home/'.",
						new[] { validationContext.MemberName }!);
				}
				else if (directory.StartsWith("/home/", StringComparison.OrdinalIgnoreCase)
					&& (directory.EndsWith("/home/", StringComparison.OrdinalIgnoreCase)
					|| directory.EndsWith("/", StringComparison.OrdinalIgnoreCase)))
				{
					return new ValidationResult("Directory must not end with '/'.",
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

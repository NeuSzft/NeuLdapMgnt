﻿using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models.CustomValidationAttributes.TeacherAttributes
{
	public class TeacherIdAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is string id)
			{
				if (!id.Contains('.'))
				{
					return new ValidationResult($"ID must contain '.'",
						new[] { validationContext.MemberName }!);
				}
				else if (id.Split('.')[0].Length < 3)
				{
					return new ValidationResult("The first part of the ID must be at least 3 characters long.",
						new[] { validationContext.MemberName }!);
				}
				else if (id.Split('.')[1].Length < 3)
				{
					return new ValidationResult("The second part of the ID must be at least 3 characters long.",
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

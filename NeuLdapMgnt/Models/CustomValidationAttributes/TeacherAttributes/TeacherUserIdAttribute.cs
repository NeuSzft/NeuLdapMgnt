﻿using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models.CustomValidationAttributes.TeacherAttributes
{
	public class TeacherUserIdAttribute : ValidationAttribute
	{
		private readonly int _min;
		private readonly int _max;

		public TeacherUserIdAttribute(int min, int max)
		{
			_min = min;
			_max = max;
		}

		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is int uid)
			{
				if (uid < _min || uid > _max)
				{
					return new ValidationResult($"ID must be between {_min} and {_max}.",
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

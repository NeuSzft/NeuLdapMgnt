using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models.CustomValidationAttributes.TeacherAttributes
{
	public class TeacherUserIdAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is int uid)
			{
				if (uid is < 4000 or > 5999)
				{
					return new ValidationResult($"ID must be between {4000} and {5999}.",
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

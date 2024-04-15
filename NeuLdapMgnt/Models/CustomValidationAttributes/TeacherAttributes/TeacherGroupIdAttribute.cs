using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models.CustomValidationAttributes.TeacherAttributes
{
	public class TeacherGroupIdAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is int gid)
			{
				if (gid is < 4000 or > 5999)
				{
					return new ValidationResult($"Group ID must be between {4000} and {5999}.",
						new[] { validationContext.MemberName }!);
				}
				else
				{
					return ValidationResult.Success;
				}
			}

			return new ValidationResult("Group ID: Invalid data type", new[] { validationContext.MemberName }!);
		}
	}
}

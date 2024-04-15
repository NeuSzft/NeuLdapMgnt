using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models.CustomValidationAttributes.StudentAttributes
{
	public class StudentGroupIdAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is int gid)
			{
				if (gid is < 6000 or > 9999)
				{
					return new ValidationResult($"Group ID must be between {6000} and {9999}.",
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

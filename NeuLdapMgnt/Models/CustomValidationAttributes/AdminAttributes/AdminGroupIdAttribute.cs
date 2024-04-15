using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models.CustomValidationAttributes.AdminAttributes
{
	public class AdminGroupIdAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is int gid)
			{
				if (gid is < 1000 or > 3999)
				{
					return new ValidationResult($"Group ID must be between {1000} and {1999}.",
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

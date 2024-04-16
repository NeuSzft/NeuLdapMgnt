using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models.CustomValidationAttributes.TeacherAttributes
{
	public class TeacherGroupIdAttribute : ValidationAttribute
	{
		private readonly int _min;
		private readonly int _max;

		public TeacherGroupIdAttribute(int min, int max)
		{
			_min = min;
			_max = max;
		}

		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is int gid)
			{
				if (gid < _min || gid > _max)
				{
					return new ValidationResult($"Group ID must be between {_min} and {_max}.",
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

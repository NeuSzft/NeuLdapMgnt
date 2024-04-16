using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models.CustomValidationAttributes.StudentAttributes
{
	public class StudentIdAttribute : ValidationAttribute
	{
		private readonly long _min;
		private readonly long _max;

		public StudentIdAttribute(long min, long max)
		{
			_min = min;
			_max = max;
		}

		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is long id)
			{
				if (id < _min || id > _max)
				{
					return new ValidationResult($"OM must be between {_min} and {_max}.",
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

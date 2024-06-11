using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models.CustomValidationAttributes.NameAttributes
{
    public class SurnameAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string lastName)
            {
                if (lastName.Length < 3)
                {
                    return new ValidationResult("Surname must be at least 3 characters long.",
                        new[] { validationContext.MemberName }!);
                }
                else
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult("Surname: Invalid data type", new[] { validationContext.MemberName }!);
        }
    }
}

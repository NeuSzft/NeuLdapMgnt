using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.Models.CustomValidationAttributes.NameAttributes
{
    public class GivenNameAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string firstName)
            {
                if (firstName.Length < 3)
                {
                    return new ValidationResult("First name must be at least 3 characters long.",
                        new[] { validationContext.MemberName }!);
                }
                else
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult("First name: Invalid data type", new[] { validationContext.MemberName }!);
        }
    }
}

using ChineseSaleApi.Dto;
using ChineseSaleApi.Services;
using System.ComponentModel.DataAnnotations;

namespace ChineseSaleApi.Validations
{
    public class DateValidation:ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime date && date <= DateTime.Now)
            {
                return new ValidationResult("Lottery start date must be in the future.");
            }
            return ValidationResult.Success;
        }
    }
}

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AdminSystem.Models
{
    public class PhoneFormatAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            var phone = value as string;
            if (string.IsNullOrEmpty(phone)) return ValidationResult.Success;
            return Regex.IsMatch(phone, @"^\d{4}-\d{6}$")
                ? ValidationResult.Success
                : new ValidationResult("請使用格式 xxxx-xxxxxx");
        }
    }
}
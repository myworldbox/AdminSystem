using System.ComponentModel.DataAnnotations;
using System.Linq;
using AdminSystem.Application.ViewModels;
using AdminSystem.Infrastructure.Data;

namespace AdminSystem.Application.Validators
{
    public class UniqueEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object value, ValidationContext validationContext)
        {
            var model = validationContext.ObjectInstance as ContactViewModel;
            var dbContext = validationContext.GetService<AppDbContext>();

            if (model == null || dbContext == null || string.IsNullOrEmpty(model.Email))
                return ValidationResult.Success;

            var exists = dbContext.客戶聯絡人
                .Any(c => c.客戶id == model.客戶Id && c.Email == model.Email && c.Id != model.Id);

            return exists
                ? new ValidationResult("同客戶下Email不能重複")
                : ValidationResult.Success;
        }
    }
}

using AdminSystem.Application.ViewModels;
using AdminSystem.Domain.Entities;
using AdminSystem.Infrastructure.Repositories;
using System.ComponentModel.DataAnnotations;

public class UniqueEmailAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object value, ValidationContext validationContext)
    {
        var model = validationContext.ObjectInstance as ContactViewModel;
        var unitOfWork = validationContext.GetService<IUnitOfWork>();

        if (model == null || unitOfWork == null || unitOfWork.Contacts == null || string.IsNullOrEmpty(model.Email))
            return ValidationResult.Success;

        var exists = unitOfWork.Contacts.Get()
               .Any(c => c.客戶Id == model.客戶Id && c.Email == model.Email && c.Id != model.Id);

        return exists
            ? new ValidationResult("同客戶下Email不能重複")
            : ValidationResult.Success;
    }
}

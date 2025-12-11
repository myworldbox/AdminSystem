using AdminSystem.Application.Dtos;
using AdminSystem.Application.ViewModels;
using AdminSystem.Domain.Entities;

namespace AdminSystem.Application.Services;

public interface IContactService
{
    Task<PagedResultDto<ContactViewModel>> GetPagedAsync(SearchDto searchDto);
    Task<ContactViewModel?> GetByIdAsync(int id);
    Task<ContactViewModel> GetForCreateAsync();
    Task<ContactViewModel> GetForEditAsync(int id);
    Task CreateAsync(ContactViewModel model);
    Task UpdateAsync(ContactViewModel model);
    Task DeleteAsync(int id);

    IAsyncEnumerable<客戶聯絡人> GetAllForExport(SearchDto searchDto);
    Task<ContactDropdown> PopulateDropdownAsync();
}
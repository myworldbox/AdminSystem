using AdminSystem.Application.Dtos;
using AdminSystem.Application.ViewModels;
using AdminSystem.Domain.Entities;

namespace AdminSystem.Application.Services;

public interface IBankService
{
    Task<PagedResultDto<BankViewModel>> GetPagedAsync(SearchDto searchDto);
    Task<BankViewModel?> GetByIdAsync(int id);
    Task<BankViewModel> GetForCreateAsync();
    Task<BankViewModel> GetForEditAsync(int id);
    Task CreateAsync(BankViewModel model);
    Task UpdateAsync(BankViewModel model);
    Task DeleteAsync(int id);

    IAsyncEnumerable<客戶銀行資訊> GetAllForExport(SearchDto searchDto);
    Task<BankDropdown> PopulateDropdownAsync();
}
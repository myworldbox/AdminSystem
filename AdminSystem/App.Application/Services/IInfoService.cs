using AdminSystem.Application.Dtos;
using AdminSystem.Application.ViewModels;
using AdminSystem.Domain.Entities;

namespace AdminSystem.Application.Services;

public interface IInfoService
{
    Task<PagedResultDto<InfoViewModel>> GetPagedAsync(SearchDto searchDto);
    Task<InfoViewModel?> GetByIdAsync(int id);
    Task<InfoViewModel> GetForCreateAsync();
    Task<InfoViewModel> GetForEditAsync(int id);
    Task CreateAsync(InfoViewModel model);
    Task UpdateAsync(InfoViewModel model);
    Task DeleteAsync(int id);

    // Streaming export → no memory explosion
    IAsyncEnumerable<客戶資料> GetAllForExport(SearchDto searchDto);

    // Helper used by controller when re-showing invalid forms
    Task<InfoDropdown> PopulateDropdownAsync();
}
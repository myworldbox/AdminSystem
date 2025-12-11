using AdminSystem.Application.Dtos;
using AdminSystem.Application.ViewModels;
using AdminSystem.Domain;
using AdminSystem.Domain.Entities;
using AdminSystem.Infrastructure.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdminSystem.Application.Services;

public class InfoService : IInfoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InfoService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    private IQueryable<客戶資料> GetBaseQuery(SearchDto searchDto)
    {
        var query = _unitOfWork.Infos.Get();

        if (!string.IsNullOrEmpty(searchDto.SearchTerm))
        {
            var term = searchDto.SearchTerm.ToUpper();
            query = query.Where(c =>
                c.客戶名稱.Contains(term) ||
                c.統一編號.Contains(term) ||
                c.電話.Contains(term) ||
                c.地址.Contains(term) ||
                c.Email.Contains(term));
        }

        query = searchDto.Order switch
        {
            Enums.Order.desc => query.OrderByDescending(x => x.Id),
            _ => query.OrderBy(x => x.Id)
        };

        return query;
    }

    public async Task<PagedResultDto<InfoViewModel>> GetPagedAsync(SearchDto searchDto)
    {
        var query = GetBaseQuery(searchDto);

        var total = await query.CountAsync();

        var items = await query
            .Skip((searchDto.Page - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .ToListAsync();

        var data = _mapper.Map<List<InfoViewModel>>(items);

        return new PagedResultDto<InfoViewModel>
        {
            Items = data,
            TotalRecords = total,
            SearchDto = searchDto
        };
    }

    public Task<InfoViewModel?> GetByIdAsync(int id)
        => _unitOfWork.Infos.GetByIdAsync(id)
            .ContinueWith(t => t.Result == null ? null : _mapper.Map<InfoViewModel>(t.Result));

    public async Task<InfoViewModel> GetForCreateAsync()
        => new() { dropdown = await PopulateDropdownAsync() };

    public async Task<InfoViewModel> GetForEditAsync(int id)
    {
        var entity = await _unitOfWork.Infos.GetByIdAsync(id)
                     ?? throw new KeyNotFoundException($"Id {id} 不存在");

        var vm = _mapper.Map<InfoViewModel>(entity);
        vm.dropdown = await PopulateDropdownAsync();
        return vm;
    }

    public async Task CreateAsync(InfoViewModel model)
    {
        var entity = _mapper.Map<客戶資料>(model);
        await _unitOfWork.Infos.InsertAsync(entity);
        await _unitOfWork.SaveAsync();
    }

    public async Task UpdateAsync(InfoViewModel model)
    {
        var entity = _mapper.Map<客戶資料>(model);
        await _unitOfWork.Infos.UpdateAsync(entity);
        await _unitOfWork.SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await _unitOfWork.Infos.SoftDeleteAsync(id);
        await _unitOfWork.SaveAsync();
    }

    public async IAsyncEnumerable<客戶資料> GetAllForExport(SearchDto searchDto)
    {
        var query = GetBaseQuery(searchDto);

        await foreach (var entity in query.AsAsyncEnumerable())
        {
            yield return new 客戶資料 {
                客戶名稱 = entity.客戶名稱,
                統一編號 = entity.統一編號,
                電話 = entity.電話,
                傳真 = entity.傳真,
                地址 = entity.地址,
                Email = entity.Email,
                客戶分類 = entity.客戶分類
            };
        }
    }

    public Task<InfoDropdown> PopulateDropdownAsync()
    {
        var list = _unitOfWork.Infos.Get().Select(x => new { x.Id, x.客戶名稱 });
        return Task.FromResult(new InfoDropdown
        {
            客戶IdList = new SelectList(list, "Id", "客戶名稱"),
            客戶分類List = new SelectList(Enum.GetValues(typeof(Enums.Category)))
        });
    }
}
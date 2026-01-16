using AdminSystem.App.Application.Interfaces;
using AdminSystem.App.Infrastructure.Interfaces;
using AdminSystem.Application.Dtos;
using AdminSystem.Application.ViewModels;
using AdminSystem.Domain;
using AdminSystem.Domain.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AdminSystem.App.Application.Services;

public class BankService : IBankService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BankService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    private IQueryable<客戶銀行資訊> GetBaseQuery(SearchDto searchDto)
    {
        var query = _unitOfWork.Banks.Get().Include(b => b.客戶).AsQueryable();

        if (!string.IsNullOrEmpty(searchDto.SearchTerm))
        {
            var term = searchDto.SearchTerm.ToUpper();
            query = query.Where(b =>
                b.客戶.客戶名稱.Contains(term) ||
                b.銀行名稱.Contains(term) ||
                b.銀行代碼.ToString().Contains(term) ||
                b.分行代碼.ToString()!.Contains(term) ||
                b.帳戶名稱.Contains(term) ||
                b.帳戶號碼.Contains(term));
        }

        Expression<Func<客戶銀行資訊, object>> orderExpr = searchDto.OrderName switch
        {
            nameof(客戶資料.客戶名稱) => c => c.客戶.客戶名稱,
            nameof(客戶銀行資訊.銀行名稱) => c => c.銀行名稱,
            nameof(客戶銀行資訊.銀行代碼) => c => c.銀行代碼,
            nameof(客戶銀行資訊.分行代碼) => c => c.分行代碼!,
            nameof(客戶銀行資訊.帳戶名稱) => c => c.帳戶名稱,
            nameof(客戶銀行資訊.帳戶號碼) => c => c.帳戶號碼,
            _ => c => c.Id
        };

        query = searchDto.Order == Enums.Order.Desc
            ? query.OrderByDescending(orderExpr)
            : query.OrderBy(orderExpr);

        return query;
    }

    public async Task<PagedResultDto<BankViewModel>> GetPagedAsync(SearchDto searchDto)
    {
        var query = GetBaseQuery(searchDto);
        var total = await query.CountAsync();

        var items = await query
            .Skip((searchDto.Page - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .ToListAsync();

        var data = _mapper.Map<List<BankViewModel>>(items);
;
        return new PagedResultDto<BankViewModel>
        {
            Items = data,
            TotalRecords = total,
            SearchDto = searchDto
        };
    }

    public Task<BankViewModel?> GetByIdAsync(int id)
        => _unitOfWork.Banks.Queryable().Include(b => b.客戶).FirstOrDefaultAsync(x => x.Id == id)
            .ContinueWith(t => t.Result == null ? null : _mapper.Map<BankViewModel>(t.Result));

    public async Task<BankViewModel> GetForCreateAsync()
        => new() { dropdown = await PopulateDropdownAsync() };

    public async Task<BankViewModel> GetForEditAsync(int id)
    {
        var entity = await GetByIdAsync(id);

        var vm = _mapper.Map<BankViewModel>(entity);
        vm.dropdown = await PopulateDropdownAsync();
        return vm;
    }

    public async Task CreateAsync(BankViewModel model)
    {
        var entity = _mapper.Map<客戶銀行資訊>(model);
        await _unitOfWork.Banks.InsertAsync(entity);
        await _unitOfWork.SaveAsync();
    }

    public async Task UpdateAsync(BankViewModel model)
    {
        var entity = _mapper.Map<客戶銀行資訊>(model);
        await _unitOfWork.Banks.UpdateAsync(entity);
        await _unitOfWork.SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await _unitOfWork.Banks.SoftDeleteAsync(id);
        await _unitOfWork.SaveAsync();
    }

    public async IAsyncEnumerable<客戶銀行資訊> GetAllForExport(SearchDto searchDto)
    {
        var query = GetBaseQuery(searchDto);

        await foreach (var entity in query.AsAsyncEnumerable())
        {
            yield return new 客戶銀行資訊
            {
                客戶 = entity.客戶,
                客戶Id = entity.客戶Id,
                銀行名稱 = entity.銀行名稱,
                銀行代碼 = entity.銀行代碼,
                分行代碼 = entity.分行代碼,
                帳戶名稱 = entity.帳戶名稱,
                帳戶號碼 = entity.帳戶號碼
            };
        }
    }

    public async Task<BankDropdown> PopulateDropdownAsync()
    {
        var customers = await _unitOfWork.Infos.Get()
            .Select(c => new { c.Id, c.客戶名稱 })
            .ToListAsync();

        return new BankDropdown
        {
            客戶IdList = new SelectList(customers, "Id", "客戶名稱")
        };
    }
}
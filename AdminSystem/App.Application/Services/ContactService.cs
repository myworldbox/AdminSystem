using AdminSystem.Application.Dtos;
using AdminSystem.Application.ViewModels;
using AdminSystem.Domain;
using AdminSystem.Domain.Entities;
using AdminSystem.Infrastructure.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdminSystem.Application.Services;

public class ContactService : IContactService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ContactService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    private IQueryable<客戶聯絡人> GetBaseQuery(SearchDto searchDto)
    {
        var query = _unitOfWork.Contacts.Get();

        if (!string.IsNullOrEmpty(searchDto.SearchTerm))
        {
            var term = searchDto.SearchTerm.ToUpper();
            query = query.Where(c =>
                c.姓名.Contains(term) ||
                c.Email.Contains(term) ||
                (c.職稱 != null && c.職稱.Contains(term)) ||
                (c.手機 != null && c.手機.Contains(term)) ||
                (c.電話 != null && c.電話.Contains(term)));
        }

        query = searchDto.OrderName switch
        {
            "姓名" => searchDto.Order == Enums.Order.Desc ? query.OrderByDescending(x => x.姓名) : query.OrderBy(x => x.姓名),
            "Email" => searchDto.Order == Enums.Order.Desc ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
            "職稱" => searchDto.Order == Enums.Order.Desc ? query.OrderByDescending(x => x.職稱) : query.OrderBy(x => x.職稱),
            _ => searchDto.Order == Enums.Order.Desc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
        };

        return query;
    }

    public async Task<PagedResultDto<ContactViewModel>> GetPagedAsync(SearchDto searchDto)
    {
        var query = GetBaseQuery(searchDto);
        var total = await query.CountAsync();

        var items = await query
            .Skip((searchDto.Page - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .ToListAsync();

        var data = _mapper.Map<List<ContactViewModel>>(items);

        return new PagedResultDto<ContactViewModel>
        {
            Items = data,
            TotalRecords = total,
            SearchDto = searchDto
        };
    }

    public Task<ContactViewModel?> GetByIdAsync(int id)
        => _unitOfWork.Contacts.GetByIdAsync(id)
            .ContinueWith(t => t.Result == null ? null : _mapper.Map<ContactViewModel>(t.Result));

    public async Task<ContactViewModel> GetForCreateAsync()
        => new() { dropdown = await PopulateDropdownAsync() };

    public async Task<ContactViewModel> GetForEditAsync(int id)
    {
        var entity = await _unitOfWork.Contacts.GetByIdAsync(id)
                     ?? throw new KeyNotFoundException($"聯絡人 Id {id} 不存在");

        var vm = _mapper.Map<ContactViewModel>(entity);
        vm.dropdown = await PopulateDropdownAsync();
        return vm;
    }

    public async Task CreateAsync(ContactViewModel model)
    {
        var entity = _mapper.Map<客戶聯絡人>(model);
        await _unitOfWork.Contacts.InsertAsync(entity);
        await _unitOfWork.SaveAsync();
    }

    public async Task UpdateAsync(ContactViewModel model)
    {
        var entity = _mapper.Map<客戶聯絡人>(model);
        await _unitOfWork.Contacts.UpdateAsync(entity);
        await _unitOfWork.SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await _unitOfWork.Contacts.SoftDeleteAsync(id);
        await _unitOfWork.SaveAsync();
    }

    public async IAsyncEnumerable<客戶聯絡人> GetAllForExport(SearchDto searchDto)
    {
        var query = GetBaseQuery(searchDto).Include(b => b.客戶);

        await foreach (var entity in query.AsAsyncEnumerable())
        {
            yield return new 客戶聯絡人
            {
                客戶 = entity.客戶,
                客戶Id = entity.客戶Id,
                職稱 = entity.職稱,
                姓名 = entity.姓名,
                Email = entity.Email,
                手機 = entity.手機,
                電話 = entity.電話
            };
        }
    }

    public async Task<ContactDropdown> PopulateDropdownAsync()
    {
        var customers = await _unitOfWork.Infos.Get()
            .Select(c => new { c.Id, c.客戶名稱 })
            .ToListAsync();

        return new ContactDropdown
        {
            客戶IdList = new SelectList(customers, "Id", "客戶名稱")
        };
    }
}
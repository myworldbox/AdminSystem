using AdminSystem.Application.Dtos;
using AdminSystem.Application.ViewModels;
using AdminSystem.Domain.Entities;
using AdminSystem.Infrastructure.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AdminSystem.Application.Services;

public class SummaryService : ISummaryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SummaryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<VwCustomerSummary>> GetSummaryAsync()
    {
        var query = _unitOfWork.Infos.Get() // 假設這是 DbSet<客戶資料>
            .AsNoTracking()
            .Where(c => !c.是否已刪除);

        var result = await query
            .Select(c => new VwCustomerSummary
            {
                Id = c.Id,
                客戶名稱 = c.客戶名稱,
                聯絡人數量 = c.客戶聯絡人s.Count(cl => !cl.是否已刪除),
                銀行帳戶數量 = c.客戶銀行資訊s.Count(cb => !cb.是否已刪除)
            })
            .ToListAsync();

        return result;
    }
}
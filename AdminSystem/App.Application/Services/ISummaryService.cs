using AdminSystem.Application.Dtos;
using AdminSystem.Application.ViewModels;
using AdminSystem.Domain.Entities;

namespace AdminSystem.Application.Services;

public interface ISummaryService
{
    Task<List<VwCustomerSummary>> GetSummaryAsync();
}
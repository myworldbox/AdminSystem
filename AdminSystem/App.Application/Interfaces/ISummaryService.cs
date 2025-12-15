using AdminSystem.Application.Dtos;
using AdminSystem.Application.ViewModels;
using AdminSystem.Domain.Entities;

namespace AdminSystem.App.Application.Interfaces;

public interface ISummaryService
{
    Task<List<VwCustomerSummary>> GetSummaryAsync();
}
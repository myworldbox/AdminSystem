using AutoMapper;
using AdminSystem.Domain.Entities;
using AdminSystem.Application.ViewModels;

namespace AdminSystem.Application.Helpers;

public class MappingHelper : Profile
{
    public MappingHelper()
    {
        CreateMap<客戶資料, InfoViewModel>();
        CreateMap<InfoViewModel, 客戶資料>();

        CreateMap<客戶聯絡人, ContactViewModel>();
        CreateMap<ContactViewModel, 客戶聯絡人>();

        CreateMap<客戶銀行資訊, BankViewModel>();
        CreateMap<BankViewModel, 客戶銀行資訊>();

        CreateMap<VwCustomerSummary, SummaryViewModel>();
        CreateMap<SummaryViewModel, VwCustomerSummary>();
    }
}
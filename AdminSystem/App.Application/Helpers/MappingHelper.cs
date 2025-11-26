using AutoMapper;
using AdminSystem.Domain.Entities;
using AdminSystem.Application.ViewModels;

namespace AdminSystem.Application.Helpers;

public class MappingHelper : Profile
{
    public MappingHelper()
    {
        CreateMap<客戶資料, InfoViewModel>().ReverseMap();

        CreateMap<客戶聯絡人, ContactViewModel>().ReverseMap();

        CreateMap<客戶銀行資訊, BankViewModel>().ReverseMap();

        CreateMap<VwCustomerSummary, SummaryViewModel>().ReverseMap();
    }
}
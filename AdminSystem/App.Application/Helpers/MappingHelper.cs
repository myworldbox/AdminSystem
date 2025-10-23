using AutoMapper;
using AdminSystem.Domain.Entities;
using AdminSystem.Application.ViewModels;

namespace AdminSystem.Application.Helpers;

public class MappingHelper : Profile
{
    public MappingHelper()
    {
        CreateMap<�Ȥ���, InfoViewModel>();
        CreateMap<InfoViewModel, �Ȥ���>();

        CreateMap<�Ȥ��p���H, ContactViewModel>();
        CreateMap<ContactViewModel, �Ȥ��p���H>();

        CreateMap<�Ȥ�Ȧ��T, BankViewModel>();
        CreateMap<BankViewModel, �Ȥ�Ȧ��T>();

        CreateMap<VwCustomerSummary, SummaryViewModel>();
        CreateMap<SummaryViewModel, VwCustomerSummary>();
    }
}
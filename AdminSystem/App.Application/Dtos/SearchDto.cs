using AdminSystem.Application.ViewModels;
using AdminSystem.Domain;
using AdminSystem.Domain.Entities;
using AutoMapper;

namespace AdminSystem.Application.Dtos
{
    public record SearchDto(
        string? SearchTerm = null,
        string OrderName = "Id",
        Enums.Order Order = Enums.Order.asc,
        int Page = 1,
        int PageSize = 10
    );
}
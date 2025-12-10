using AdminSystem.Application.ViewModels;
using AdminSystem.Domain;
using AdminSystem.Domain.Entities;
using AutoMapper;

namespace AdminSystem.Application.Dtos
{
    public record SearchDto(
        string SearchTerm,
        Enums.Order Order,
        int Page,
        int PageSize = 10
    );
}
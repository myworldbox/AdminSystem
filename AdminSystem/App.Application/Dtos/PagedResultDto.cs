using AutoMapper;
using AdminSystem.Domain.Entities;
using AdminSystem.Application.ViewModels;

namespace AdminSystem.Application.Dtos;

public class PagedResultDto<T> : PaginationDto
{
    public IReadOnlyList<T>? Items { get; set; }
}
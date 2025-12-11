using AutoMapper;
using AdminSystem.Domain.Entities;
using AdminSystem.Application.ViewModels;

namespace AdminSystem.Application.Dtos;

public class PaginationDto
{
    public int TotalRecords { get; set; }
    public SearchDto SearchDto { get; set; } = new();
    public int TotalPages => SearchDto.PageSize == 0 ? 1 : (int)Math.Ceiling((double)TotalRecords / SearchDto.PageSize);
}
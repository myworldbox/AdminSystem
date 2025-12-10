using AutoMapper;
using AdminSystem.Domain.Entities;
using AdminSystem.Application.ViewModels;

namespace AdminSystem.Application.Dtos;

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalRecords { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
}
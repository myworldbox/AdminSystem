using AdminSystem.Application.Services;
using AdminSystem.Application.ViewModels;
using AdminSystem.Infrastructure.Data;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AdminSystem.Web.Controllers
{
    public class SummaryController(ISummaryService _summaryService, IMapper _mapper) : Controller
    {
        public async Task<IActionResult> IndexAsync()
        {
            var summaries = await _summaryService.GetSummaryAsync();

            var data = _mapper.Map<IEnumerable<SummaryViewModel>>(summaries);

            ViewData["Title"] = "Summary";

            return View(data);
        }
    }
}

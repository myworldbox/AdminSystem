using AdminSystem.Application.ViewModels;
using AdminSystem.Infrastructure.Data;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AdminSystem.Web.Controllers
{
    public class SummaryController(AppDbContext _context, IMapper _mapper) : Controller
    {
        public IActionResult Index()
        {
            var summaries = _context.vw_CustomerSummary.ToList();

            ViewData["Title"] = "Summary";

            var data = _mapper.Map<IEnumerable<SummaryViewModel>>(summaries);

            return View(data);
        }
    }
}

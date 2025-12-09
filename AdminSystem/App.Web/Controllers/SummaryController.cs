using AdminSystem.Application.ViewModels;
using AdminSystem.Infrastructure.Data;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AdminSystem.Web.Controllers
{
    public class SummaryController(AppDbContext context, IMapper mapper) : Controller
    {
        public IActionResult Index()
        {
            var summaries = context.vw_CustomerSummary.ToList();

            ViewData["Title"] = "Summary";

            var data = mapper.Map<IEnumerable<SummaryViewModel>>(summaries);

            return View(data);
        }
    }
}

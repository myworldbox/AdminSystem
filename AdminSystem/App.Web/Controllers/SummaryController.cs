using AdminSystem.Application.ViewModels;
using AdminSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace AdminSystem.Web.Controllers
{
    public class SummaryController : Controller
    {
        private readonly AppDbContext _context;

        public SummaryController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var summaries = _context.vw_CustomerSummary.ToList();

            ViewData["Title"] = "Summary";

            return View(summaries);
        }
    }
}

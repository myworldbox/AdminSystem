using AdminSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdminSystem.Controllers
{
    public class SummaryController : Controller
    {
        private readonly CustomerEntities _context;

        public SummaryController(CustomerEntities context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var summaries = _context.vw_CustomerSummary.ToList();
            return View(summaries);
        }
    }
}

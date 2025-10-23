using AdminSystem.Application.ViewModels;
using AdminSystem.Infrastructure.Data;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AdminSystem.Web.Controllers
{
    public class SummaryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public SummaryController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            var summaries = _context.vw_CustomerSummary.ToList();

            ViewData["Title"] = "Summary";

            var data = _mapper.Map<IEnumerable<SummaryViewModel>>(summaries);

            return View(data);
        }
    }
}

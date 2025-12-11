using AdminSystem.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AdminSystem.Web.Controllers
{
    public class HomeController(ILogger<HomeController> logger) : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Home Page";
            return View();
        }

        public IActionResult Privacy()
        {
            ViewData["Title"] = "Privacy Policy";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            ViewData["Title"] = "Error";
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

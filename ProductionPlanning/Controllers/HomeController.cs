using ProductionPlanning.Models;
using ProductionPlanning.Models.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ProductionPlanning.ViewModel.Request;

namespace ProductionPlanning.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public HomeController(ILogger<HomeController> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole(Role.Administrator) || User.IsInRole(Role.Seller)
                || User.IsInRole(Role.Manufacture) || User.IsInRole(Role.Header))
            {
                return RedirectToAction("RequestMonth", "Request");
            }
            else if (User.IsInRole(Role.Accountant))
            {
                return RedirectToAction("AccounterOrders", "Request");
            }

            return View();
        }

        [Authorize(Roles = Role.Administrator)]
        public IActionResult Privacy()
        {
            //for (int i = 0; i < 10000; i++)
            //{
            //    _logger.LogInformation("Privacy Information");
            //    _logger.LogError("Privacy Error");
            //}
            return View();
        }

        [Authorize(Roles = Role.Administrator)]
        [Authorize(Roles = Role.Seller)]
        public IActionResult AddProductionRequest(int id = 0)
        {
            return RedirectToAction("AddProductionRequest", "Request", new { _id = id });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

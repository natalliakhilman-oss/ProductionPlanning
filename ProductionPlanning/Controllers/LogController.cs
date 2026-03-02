using ProductionPlanning.Models.Logging;
using ProductionPlanning.ViewModel.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using ProductionPlanning.Models;

namespace ProductionPlanning.Controllers
{
    [Authorize]
    public class LogController : Controller
    {
        private readonly ILogger<LogController> _logger;

        public LogController(ILogger<LogController> logger)
        {
            _logger = logger;
        }

        [Authorize(Roles = Role.Administrator)]
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetLogsPartial()
        {
            try
            {
                LogFileReader logReader = new LogFileReader();
                List<string> logs = await logReader.ReadAllLinesAsync();

                var model = new LogsViewModel
                {
                    Logs = logs
                };

                return PartialView("_LogsPartialView", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading logs partial");
                return PartialView("_ErrorPartial", ex.Message);
            }
        }
    }
}

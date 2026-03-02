using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProductionPlanning.Extensions;
using ProductionPlanning.Models;
using ProductionPlanning.ViewModel.Equipments;
using ProductionPlanning.ViewModel.User;
using System.Security.Claims;
using static ProductionPlanning.Models.Equipment;
using static ProductionPlanning.Models.ProductRequest;
using static System.Formats.Asn1.AsnWriter;

namespace ProductionPlanning.Controllers
{
    [Authorize]
    public class EquipmentController : Controller
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EquipmentController> _logger;

        public EquipmentController(ILogger<EquipmentController> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        [Authorize(Roles = Role.Administrator)]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = Role.Administrator)]
        public async Task<IActionResult> Equipments()
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                    var eqipments = await dbContext.Equipments
                        .AsNoTracking()
                        .Where(e => !e.IsDeleted)
                        .ToListAsync();

                    var model = new EquipmentsViewModel(eqipments);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View("BadRequest", ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = Role.Administrator)]
        public IActionResult AddEquipment()
        {
            try
            {
                ViewBag.eqType = new SelectList(
                        Enum.GetValues(typeof(EquipmentType))
                                    .Cast<EquipmentType>()
                                    .Select(t => new
                                    {
                                        Value = t,
                                        Text = t.ToStringX()
                                    })
                                    .ToList(),
                        "Value",
                        "Text"
                    );

                AddEquipmentViewModel model = new AddEquipmentViewModel();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View("BadRequest", ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = Role.Administrator)]
        public async Task<IActionResult> AddEquipment(AddEquipmentViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                        var lastId = await dbContext.Equipments
                            .AsNoTracking()
                            .MaxAsync(e => e.Id);

                        model.Equipment.Id = lastId + 1;

                        await dbContext.Equipments.AddAsync(model.Equipment);
                        await dbContext.SaveChangesAsync();

                        return RedirectToAction("Equipments");
                    }
                }

                return View("BadRequest", "Некорректные данные");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View("BadRequest", ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = Role.Administrator)]
        public async Task<IActionResult> EditEquipment(int id)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                    var equipment = await dbContext.Equipments
                        .AsNoTracking()
                        .Where(e => e.Id == id)
                        .FirstOrDefaultAsync();

                    if (equipment == null) 
                    {
                        return View("BadRequest", "Оборудование не найлено");
                    }

                    ViewBag.eqType = new SelectList(
                       Enum.GetValues(typeof(EquipmentType))
                                   .Cast<EquipmentType>()
                                   .Select(t => new
                                   {
                                       Value = t,
                                       Text = t.ToStringX()
                                   })
                                   .ToList(),
                       "Value",
                       "Text"
                   );

                    var model = new EditEquipmentViewModel(equipment);

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View("BadRequest", ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = Role.Administrator)]
        public async Task<IActionResult> EditEquipment(EditEquipmentViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                        var eqipment = await dbContext.Equipments
                            .AsNoTracking()
                            .FirstOrDefaultAsync(e => e.Id == model.Equipment.Id);

                        if (eqipment == null)
                        {
                            return View("BadRequest", "Оборудование не найдено");
                        }

                        eqipment = model.Equipment;

                        dbContext.Equipments.Update(eqipment);
                        await dbContext.SaveChangesAsync();

                        return RedirectToAction("Equipments");
                    }
                }

                return View("BadRequest", "Некорректные данные");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View("BadRequest", ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = Role.Administrator)]
        public async Task<JsonResult> DeleteEquipment(int id)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                    var eqipment = await dbContext.Equipments
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);

                    if (eqipment == null)
                    {
                        return Json(new { success = false, message = "Оборудование не найдено" });
                    }

                    eqipment.IsDeleted = true;

                    dbContext.Equipments.Update(eqipment);
                    await dbContext.SaveChangesAsync();

                    return Json(new { success = true, message = "Оборудование успешно удалено" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Произошла ошибка: {ex.Message}" });
            }
        }

        // Вспомогательный метод для получения partial view
        [HttpGet]
        [Authorize(Roles = Role.Administrator)]
        public async Task<PartialViewResult> GetEquipmentsPartial()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                var eqipments = await dbContext.Equipments
                    .AsNoTracking()
                    .Where(e => !e.IsDeleted)
                    .ToListAsync();

                var model = new EquipmentsViewModel(eqipments);
                return PartialView("_EquipmentsPartialView", model);
            }
        }
    }
}

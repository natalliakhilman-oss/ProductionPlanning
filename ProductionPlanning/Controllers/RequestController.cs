using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ProductionPlanning.Extensions;
using ProductionPlanning.Hubs;
using ProductionPlanning.Models;
using ProductionPlanning.ViewModel.Request;
using System.Data;
using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using static NuGet.Packaging.PackagingConstants;
using static ProductionPlanning.Models.ProductRequest;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProductionPlanning.Controllers
{
    [Authorize]
    public class RequestController : Controller
    {
        private readonly ILogger<RequestController> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<RequestHub> _hubContext;

        public RequestController(ILogger<RequestController> logger, IServiceScopeFactory scopeFactory, IHubContext<RequestHub> hubContext)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = Role.Administrator + "," + Role.Seller)]
        public async Task<IActionResult> AddProductionRequest(int _id)
        {
            var model = new AddRequestViewModel();

            if (_id != 0)
            {
                model.App.EquipmentId = _id;
            }

            await FillAddRequestViewBagAsync(model.App.MonthDatePlanning);
            return View(model);
        }

        private async Task FillAddRequestViewBagAsync(DateTime monthDatePlanning)
        {
            ViewBag.rType = new SelectList(
                Enum.GetValues(typeof(ProductionRequestType))
                    .Cast<ProductionRequestType>()
                    .Where(t => t != ProductionRequestType.No)
                    .Select(t => new { Value = t, Text = t.ToStringX() })
                    .ToList(),
                "Value", "Text");

            var months = new List<SelectListItem>();
            for (int i = 0; i < 12; i++)
            {
                var date = monthDatePlanning.AddMonths(i);
                months.Add(new SelectListItem
                {
                    Value = date.ToString("yyyy-MM"),
                    Text = date.ToString("MMMM yyyy", new CultureInfo("ru-RU"))
                });
            }
            ViewBag.Months = months;

            ViewBag.urgency = new SelectList(
                Enum.GetValues(typeof(ProductionRequestUrgency))
                    .Cast<ProductionRequestUrgency>()
                    .Select(t => new { Value = t, Text = t.ToStringX() })
                    .ToList(),
                "Value", "Text");

            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                var equpList = await dbContext.Equipments.AsNoTracking().Where(f => f.IsDeleted != true).ToListAsync();
                ViewBag.equipmentList = equpList != null
                    ? new SelectList(equpList.Select(e => new { Value = e.Id, Text = e.Name }).ToList(), "Value", "Text")
                    : new SelectList(Enumerable.Empty<SelectListItem>());

                var notes = await dbContext.Notes
                    .AsNoTracking()
                    .Where(f => f.IsDeleted != true)
                    .OrderByDescending(n => n.DateCreate)
                    .ToListAsync();
                ViewBag.noteList = notes != null
                    ? new SelectList(notes.Select(e => new { Value = e.Id, Text = $"{e.GetNumString()} - {e.Name}" }).ToList(), "Value", "Text")
                    : new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }

        [HttpPost]
        [Authorize(Roles = Role.Administrator + "," + Role.Seller)]
        public async Task<IActionResult> AddProductionRequest(AddRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await FillAddRequestViewBagAsync(model.App.MonthDatePlanning);
                return View(model);
            }
            try
            {
                DateTime? monthReturn = DateTime.Now;
                var currentDate = DateTime.Now;

                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                    int? maxNumber = await dbContext.ProductRequests
                        .AsNoTracking()
                        .Where(n => n.DateCreate.Year == currentDate.Year
                                    && n.DateCreate.Month == currentDate.Month
                                    && n.DateCreate.Day == currentDate.Day
                                    && !n.IsDeleted)
                        .MaxAsync(n => (int?)n.DayNumber);

                    int resultMaxNum = maxNumber ?? 0;

                    model.App.DayNumber = resultMaxNum + 1;
                    model.App.Status = ProductRequestStatus.Created;
                    if (!model.IsUseNote)
                    {
                        model.App.NoteId = null;
                    }
                    dbContext.ProductRequests.Add(model.App);
                    await dbContext.SaveChangesAsync();

                    int requestCount = await dbContext.ProductRequests
                        .AsNoTracking()
                        .CountAsync(x => x.Status == ProductRequestStatus.Created
                            && !x.IsDeleted);

                    await _hubContext.Clients.All.SendAsync("ReceiveRequestCount", requestCount);

                    monthReturn = model.App.MonthDatePlanning;
                }

                return RedirectToAction("RequestMonth", new { _date = monthReturn });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["ErrorMessage"] = $"Ошибка данных: {ex.Message}";
                await FillAddRequestViewBagAsync(model.App.MonthDatePlanning);
                return View(model);
            }
        }

        [Authorize(Roles = Role.Administrator + "," + Role.Seller + "," + Role.Manufacture)]
        public async Task<IActionResult> RequestTable()
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                    // Requests
                    var requests = await dbContext.ProductRequests
                        .AsNoTracking()
                        .Include(p => p.Note)
                        .Include(p => p.Equipment)
                        .Where(n => !n.IsDeleted)
                        .OrderByDescending(d => d.DateCreate)
                        .ToListAsync();

                    var model = new RequestTabaleViewModel(requests);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View("BadRequest", $"Ошибка данных : {ex.Message}");

            }
        }

        [Authorize(Roles = Role.Administrator + "," + Role.Seller + "," + Role.Manufacture)]
        public async Task<IActionResult> EditRequest(Guid _id)
        {
            try
            {
                bool IsManufacture = User.IsInRole(Role.Manufacture);
                bool IsSeller = User.IsInRole(Role.Seller);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                    // Request
                    var requests = await dbContext.ProductRequests
                        .AsNoTracking()
                        .Include(p => p.Note)
                        .Include(p => p.Equipment)
                        .Where(n => !n.IsDeleted)
                        .FirstOrDefaultAsync(r => r.Id == _id);

                    if (requests == null)
                        return NotFound("Заявка не найдена");

                    var model = new EditRequestViewModel(requests);

                    #region ViewBags
                    // Urgency
                    ViewBag.urgency = new SelectList(
                        Enum.GetValues(typeof(ProductionRequestUrgency))
                                    .Cast<ProductionRequestUrgency>()
                                    .Select(t => new
                                    {
                                        Value = t,
                                        Text = t.ToStringX()
                                    })
                                    .ToList(),
                        "Value",
                        "Text"
                    );

                    // ProdRequestType
                    ViewBag.rType = new SelectList(
                        Enum.GetValues(typeof(ProductionRequestType))
                                    .Cast<ProductionRequestType>()
                                    .Where(t => t != ProductionRequestType.No)
                                    .Select(t => new
                                    {
                                        Value = t,
                                        Text = t.ToStringX()
                                    })
                                    .ToList(),
                        "Value",
                        "Text"
                    );

                    // MonthDatePlanning
                    var months = new List<SelectListItem>();

                    for (int i = 0; i < 12; i++)
                    {
                        var date = model.App.MonthDatePlanning.AddMonths(i);
                        months.Add(new SelectListItem
                        {
                            Value = date.ToString("yyyy-MM"), // или date.ToString("MM.yyyy")
                            Text = date.ToString("MMMM yyyy", new CultureInfo("ru-RU"))
                        });
                    }
                    ViewBag.Months = months;

                    // Equipment List
                    var equpList = await dbContext.Equipments.AsNoTracking().Where(f => f.IsDeleted != true).ToListAsync();
                    if (equpList != null)
                    {
                        ViewBag.equipmentList = new SelectList(
                            equpList.Cast<Equipment>()
                            .Select(e => new
                            {
                                Value = e.Id,
                                Text = e.Name,
                            })
                            .ToList(),
                            "Value",
                            "Text"
                        );
                    }
                    // noteList
                    var notes = await dbContext.Notes
                            .AsNoTracking()
                            .Where(f => f.IsDeleted != true)
                            .OrderByDescending(n => n.DateCreate)
                            .ToListAsync();

                    if (notes != null)
                    {
                        ViewBag.noteList = new SelectList(
                            notes.Cast<Note>()
                            .Select(e => new
                            {
                                Value = e.Id,
                                Text = $"{e.GetNumString()} - {e.Name}",
                            })
                            .ToList(),
                            "Value",
                            "Text"
                        );
                    }
                    #endregion ViewBags


                    if (IsManufacture)
                    {
                        return View("EditRequestManufacture", model);
                    }
                    else if (IsSeller)
                    {
                        return View("EditRequestSeller", model);
                    }

                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View("BadRequest", $"Ошибка данных : {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = Role.Administrator + "," + Role.Seller + "," + Role.Manufacture)]
        public async Task<IActionResult> EditRequest(EditRequestViewModel model)
        {
            bool IsManufacture = User.IsInRole(Role.Manufacture);
            bool IsSeller = User.IsInRole(Role.Seller);

            if (!ModelState.IsValid)
            {
                await FillAddRequestViewBagAsync(model.App.MonthDatePlanning);
                if (IsManufacture) return View("EditRequestManufacture", model);
                if (IsSeller) return View("EditRequestSeller", model);
                return View(model);
            }

            try
            {
                DateTime? monthReturn = DateTime.Now;

                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                    // Request
                    var request = await dbContext.ProductRequests
                        .AsNoTracking()
                        .Include(p => p.Note)
                        .Include(p => p.Equipment)
                        .Where(n => !n.IsDeleted)
                        .FirstOrDefaultAsync(r => r.Id == model.App.Id);

                    if (request == null)
                        return NotFound("Заявка не найдена");

                    if (IsManufacture)
                    {
                        request.DatePlaningStart = model.App.DatePlaningStart;
                        //request.DatePlaningFinish = model.App.DatePlaningFinish;
                        request.Status = ProductRequestStatus.Accepted;
                    }
                    else if (IsSeller)
                    {
                        request.AppType = model.App.AppType;
                        request.MonthDatePlanning = model.App.MonthDatePlanning;
                        request.Count = model.App.Count;
                        request.DateMaxFinish = model.App.DateMaxFinish;
                        request.AppUrgency = model.App.AppUrgency;
                        // If CheckBox IsNote
                        if (model.IsUseNoteChB)
                        {
                            request.NoteId = model.App.NoteId;
                            request.Note = null;
                        }
                        else
                        {
                            request.NoteId = null;
                            request.Note = null;
                        }
                        //return View("EditRequestSeller", model);
                    }                    

                    dbContext.ProductRequests.Update(request);
                    await dbContext.SaveChangesAsync();

                    monthReturn = request.MonthDatePlanning;
                }

                return RedirectToAction("RequestMonth", new { _date = monthReturn }); //Request/RequestMonth
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["ErrorMessage"] = $"Ошибка данных: {ex.Message}";
                await FillAddRequestViewBagAsync(model.App.MonthDatePlanning);
                if (IsManufacture) return View("EditRequestManufacture", model);
                if (IsSeller) return View("EditRequestSeller", model);
                return View(model);
            }
        }
        
        [Authorize(Roles = Role.Administrator + "," + Role.Manufacture)]
        public async Task<IActionResult> AddRequestOrder(Guid _rId, DateTime? _date)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                    // Request
                    var request = await dbContext.ProductRequests
                        .AsNoTracking()
                        .Include(p => p.Note)
                        .Include(p => p.Equipment)
                        .Where(n => !n.IsDeleted)
                        .FirstOrDefaultAsync(r => r.Id == _rId);

                    if (request == null) 
                    {
                        return View("BadRequest", "Заявка не найдена");
                    }

                    int SumCount = await dbContext.Orders
                        .AsNoTracking()
                        .Where(o => !o.IsDeleted && o.RequestId == _rId)
                        .SumAsync(o => o.Count);

                    var model = new AddOrderViewModel(request, SumCount, _date);

                    return View(model);
                }
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex.Message);
                return View("BadRequest", $"Ошибка данных : {ex.Message}");
            };
        }

        [HttpPost]
        [Authorize(Roles = Role.Administrator + "," + Role.Manufacture)]
        public async Task<IActionResult> AddRequestOrder(AddOrderViewModel model)
        {
            try
            {
                DateTime? monthReturn = DateTime.Now;
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                    // Request
                    var request = await dbContext.ProductRequests
                        //.AsNoTracking()
                        //.Include(p => p.Note)
                        //.Include(p => p.Equipment)
                        .Where(n => !n.IsDeleted)
                        .FirstOrDefaultAsync(r => r.Id == model.App.Id);

                    if (request == null)
                    {
                        return View("BadRequest", "Заявка не найдена");
                    }

                    // List of orders for request
                    var ordersCount = await dbContext.Orders
                        .AsNoTracking()
                        .Where(o => !o.IsDeleted && o.RequestId == model.App.Id)
                        .Select(o => o.Count)
                        .ToListAsync();
                    int SumCount = ordersCount.Sum();

                    var totalOrderCount = model.Order.Count + SumCount;

                    if(totalOrderCount >= request.Count)
                    {
                        request.Status = ProductRequestStatus.Completed;
                        request.DateFinish = DateTime.Now;
                    }
                    else
                    {
                        request.Status = ProductRequestStatus.AtWork;
                    }

                    // If didn't write down request DataPlaningStart
                    if (request.DatePlaningStart == null)
                    {
                        request.DatePlaningStart = model.Order.DateCreate;
                    }

                    var newOrder = new Order
                    {
                        RequestId = model.App.Id,
                        DateCreate = model.Order.DateCreate,
                        Count = model.Order.Count,
                        Description = model.Order.Description,
                        UserId = userId
                    };

                    await dbContext.Orders.AddAsync(newOrder);
                    await dbContext.SaveChangesAsync();

                    int oderCount = await dbContext.Orders
                        .AsNoTracking()
                        .CountAsync(x => x.StatusOrder == Order.OrderStatus.Created
                            && !x.IsDeleted);

                    // Отправляем обновление всем клиентам через SignalR
                    await _hubContext.Clients.All.SendAsync("ReceiveOrderCount", oderCount);

                    monthReturn = newOrder.DateCreate;
                }

                return RedirectToAction("RequestMonth", new { _date = monthReturn }); //Request/RequestMonth
                //return RedirectToAction("RequestTable");
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex.Message);
                return View("BadRequest", $"Ошибка данных : {ex.Message}");
            }
        }

        [Authorize(Roles = Role.Administrator + "," + Role.Manufacture)]
        public async Task<IActionResult> EditRequestOrder(Guid id, DateTime? _date)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                    // Order
                    var order = await dbContext.Orders
                        .AsNoTracking()
                        .Include(r => r.ProductRequest.Note)
                        .FirstOrDefaultAsync(o => o.Id == id);

                    if (order == null)
                    {
                        return View("BadRequest", "Заказ не найдена");
                    }

                    //// Request
                    //var request = await dbContext.ProductRequests
                    //    .AsNoTracking()
                    //    .Include(p => p.Note)
                    //    .Include(p => p.Equipment)
                    //    .Where(n => !n.IsDeleted)
                    //    .FirstOrDefaultAsync(r => r.Id == _rId);

                    if (order.ProductRequest == null)
                    {
                        return View("BadRequest", "Заявка не найдена");
                    }

                    // List of orders for request
                    var ordersCount = await dbContext.Orders
                        .AsNoTracking()
                        .Where(o => !o.IsDeleted && o.RequestId == order.ProductRequest.Id)
                        .Select(o => o.Count)
                        .ToListAsync();
                    int SumCount = ordersCount.Sum();

                    var model = new EditOrderViewModel(order, SumCount, _date);

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View("BadRequest", $"Ошибка данных : {ex.Message}");
            };
        }

        [HttpPost]
        [Authorize(Roles = Role.Administrator + "," + Role.Manufacture)]
        public async Task<IActionResult> EditRequestOrder(EditOrderViewModel model)
        {
            try
            {
                DateTime? monthReturn = DateTime.Now;
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                    // Order
                    var order = await dbContext.Orders
                        .Include(r => r.ProductRequest)
                        .FirstOrDefaultAsync(o => o.Id == model.Order.Id);
                    if (order == null)
                    {
                        return View("BadRequest", "Заказ не найдена");
                    }

                    order.Count = model.Order.Count;
                    order.Description = model.Order.Description;
                    order.UserId = userId;
                    order.DateInsert = DateTime.Now;

                    // List of orders for request
                    var ordersCount = await dbContext.Orders
                        .AsNoTracking()
                        .Where(o => !o.IsDeleted && o.RequestId == model.App.Id)
                        .Select(o => o.Count)
                        .ToListAsync();
                    int SumCount = ordersCount.Sum();

                    var totalOrderCount = SumCount - model.CurrentCount + order.Count;

                    if (totalOrderCount >= order.ProductRequest.Count)
                    {
                        order.ProductRequest.Status = ProductRequestStatus.Completed;
                        order.ProductRequest.DateFinish = DateTime.Now;
                    }
                    else
                    {
                        order.ProductRequest.Status = ProductRequestStatus.AtWork;
                        order.ProductRequest.DateFinish = null;
                    }
                    await dbContext.SaveChangesAsync();

                    monthReturn = order.DateCreate;
                }

                return RedirectToAction("RequestMonth", new { _date = monthReturn }); //Request/RequestMonth
                //return RedirectToAction("RequestTable");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View("BadRequest", $"Ошибка данных : {ex.Message}");
            }
        }

        [Authorize(Roles = Role.Administrator + "," + Role.Seller + "," + Role.Manufacture + "," + Role.Header)]
        public async Task<IActionResult> RequestMonth(DateTime? _date)
        {
            try
            {
                DateTime date = DateTime.Now;
                if (_date != null)
                {
                    date = _date.Value;
                }
                //date = date.AddMonths(1);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                    var equimpments = await dbContext.Equipments
                       .AsNoTracking()
                       .ToListAsync();

                    // Все заявки на нужный месяц/год
                    var productRequests = await dbContext.ProductRequests
                    .AsNoTracking()
                    .Include(o => o.Orders)
                    .Include(n => n.Note)
                    .Where(r => r.MonthDatePlanning.Year == date.Year
                                && r.MonthDatePlanning.Month == date.Month
                                && !r.IsDeleted)
                    .ToListAsync();

                    var model = new ProductsViewModel(date, equimpments, productRequests);

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View("BadRequest", $"{ex.Message}");
            }
        }
        public async Task<IActionResult> RequestMonthPartial(DateTime? _date)
        {
            try
            {
                DateTime date = DateTime.Now;
                if (_date != null)
                {
                    date = _date.Value;
                }
                //date = date.AddMonths(1);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                    var equimpments = await dbContext.Equipments
                       .AsNoTracking()
                       .ToListAsync();

                    // Все заявки на нужный месяц/год
                    var productRequests = await dbContext.ProductRequests
                    .AsNoTracking()
                    .Include(o => o.Orders)
                    .Include(n => n.Note)
                    .Where(r => r.MonthDatePlanning.Year == date.Year
                                && r.MonthDatePlanning.Month == date.Month
                                && !r.IsDeleted)
                    .ToListAsync();

                    var model = new ProductsViewModel(date, equimpments, productRequests);

                    return PartialView("_RequestMonthPartialView", model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View("BadRequest", $"{ex.Message}");
            }
        }

        [Authorize(Roles = Role.Administrator + "," + Role.Seller + "," + Role.Header)]
        public async Task<IActionResult> OrdersRequestDayPartial(DateTime? _date, Guid _requestId)
        {
            try
            {
                DateTime date = DateTime.Now;
                if (_date != null)
                {
                    date = _date.Value;
                }

                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                    var request = await dbContext.ProductRequests
                        .AsNoTracking()
                        .Include(n => n.Note)
                        .FirstOrDefaultAsync(r => r.Id == _requestId);

                    var orders = await dbContext.Orders
                       .AsNoTracking()
                       .Include(e => e.ProductRequest.Equipment)
                       .Include(e => e.ProductRequest.Note)
                       .Include(u => u.User)
                       .Where(r => r.DateCreate.Year == date.Year
                                && r.DateCreate.Month == date.Month
                                && r.DateCreate.Day == date.Day
                                && !r.IsDeleted
                                && r.ProductRequest.Id == _requestId)
                       .OrderByDescending(o => o.DateInsert)
                       .ToListAsync();

                    var model = new OrderListViewModel(orders,request);

                    return PartialView("_OrdersPartialView", model);
                }
            }
             catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View("BadRequest", $"{ex.Message}");
            }
        }
        
        [Authorize(Roles = Role.Administrator + "," + Role.Seller + "," + Role.Manufacture + "," + Role.Header)]
        public async Task<IActionResult> OrdersDayPartial(DateTime? _date)
        {
            try
            {
                DateTime date = DateTime.Now;
                if (_date != null)
                {
                    date = _date.Value;
                }

                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                    var orders = await dbContext.Orders
                       .AsNoTracking()
                       .Include(e => e.ProductRequest.Equipment)
                       .Include(e => e.ProductRequest.Note)
                       .Include(u => u.User)
                       .Where(r => r.DateCreate.Year == date.Year
                                && r.DateCreate.Month == date.Month
                                && r.DateCreate.Day == date.Day
                                && !r.IsDeleted)
                       .OrderByDescending(o => o.DateInsert)
                       .ToListAsync();

                    var model = new OrderListViewModel(orders);

                    return PartialView("_OrdersDayPartialView", model);
                }
            }
             catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View("BadRequest", $"{ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = Role.Administrator + "," + Role.Manufacture + "," + Role.Header)]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                var order = await dbContext.Orders
                    .Include(o => o.ProductRequest)
                    .ThenInclude(pr => pr.Orders)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                    return Json(new { success = false });

                order.IsDeleted = true;
                
                // Обновляем статус заявки после удаления (если был Выполнена)
                if(order.ProductRequest.Status == ProductRequestStatus.Completed)
                {
                    int summa = order.ProductRequest.Orders
                    .Where(d => !d.IsDeleted)
                    .Sum(c => c.Count);

                    if (summa < order.ProductRequest.Count) 
                    {
                        order.ProductRequest.Status = ProductRequestStatus.AtWork;
                        order.ProductRequest.DateFinish = null;
                    }
                }

                await dbContext.SaveChangesAsync();

                return Json(new { success = true });
            }
        }
        
        [HttpPost]
        [Authorize(Roles = Role.Administrator + "," + Role.Seller + "," + Role.Header)]
        public async Task<IActionResult> DeleteRequest(Guid id)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                var request = await dbContext.ProductRequests
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (request == null)
                    return Json(new { success = false });

                request.IsDeleted = true;
                request.Status = ProductRequestStatus.Removed;

                await dbContext.SaveChangesAsync();

                return Json(new { success = true });
            }
        }

        [Authorize(Roles = Role.Administrator + "," + Role.Seller + "," + Role.Manufacture + "," + Role.Header)]
        public async Task<IActionResult> RequestInfoPartial(Guid _requestId)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                    var request = await dbContext.ProductRequests
                        .AsNoTracking()
                        .Include(n => n.Note)
                        .FirstOrDefaultAsync(r => r.Id == _requestId);

                    if (request == null)
                    {
                        return View("BadRequest", "Заявка не найдена");
                    }

                    int totalCount = await dbContext.Orders
                        .AsNoTracking()
                        .Where(o => !o.IsDeleted && o.RequestId == _requestId)
                        .SumAsync(o => o.Count);

                    var model = new RequestInfoViewModel(request,totalCount);

                    return PartialView("_RequestInfoPartialView", model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View("BadRequest", $"{ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNewRequestCount()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                int requestCount = await dbContext.ProductRequests
                        .AsNoTracking()
                        .CountAsync(x => x.Status == ProductRequestStatus.Created 
                            && !x.IsDeleted );
                
                return Json(requestCount);
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> GetNewOrderCount()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                int oderCount = await dbContext.Orders
                        .AsNoTracking()
                        .CountAsync(x => x.StatusOrder == Order.OrderStatus.Created
                            && !x.IsDeleted);
                
                return Json(oderCount);
            }
        }

        #region Accounter Role
        [Authorize(Roles = Role.Accountant)]
        public async Task<IActionResult> AccounterOrders(int page = 1, string sortOrder = "created_desc")
        {
            const int pageSize = 20;

            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                //var query = dbContext.Orders
                //    .Include(o => o.ProductRequest)
                //    .   ThenInclude(pr => pr.Equipment)
                //    .Include(o => o.User)
                //    .Where(o => !o.IsDeleted)
                //    .OrderByDescending(o => o.DateInsert); 

                var query = dbContext.Orders
                 .Include(o => o.ProductRequest)
                 .ThenInclude(pr => pr.Equipment)
                 .Include(o => o.User)
                 .Where(o => !o.IsDeleted)
                 .OrderBy(o => o.StatusOrder != Order.OrderStatus.Created) // false (нужный статус) идут первыми
                 .ThenByDescending(o => o.DateInsert); // Затем сортировка по дате внутри каждой группы

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var orders = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var viewModel = new AccounterOrderListViewModel
                {
                    Orders = orders,
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = page,
                        TotalPages = totalPages,
                        PageSize = pageSize,
                        TotalCount = totalCount
                    },
                    CurrentSortOrder = sortOrder
                };

                return View(viewModel);
            }
        }

        // POST: api/Orders/UpdateStatus
        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusModel model)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                    var order = await dbContext.Orders.FindAsync(model.Id);
                    if (order == null)
                        return NotFound();

                    order.StatusOrder = Order.OrderStatus.Accepted;
                    //order.DateInsert = DateTime.Now;

                    await dbContext.SaveChangesAsync();

                    return Json(new
                    {
                        success = true,
                        message = "Статус успешно обновлен",
                        dateInsert = order.DateInsert.ToString("dd.MM.yyyy HH:mm")
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class UpdateStatusModel
        {
            public Guid Id { get; set; }
        }
        #endregion AccounterRole
    }
}

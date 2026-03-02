using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionPlanning.Extensions;
using ProductionPlanning.Models;

namespace ProductionPlanning.Controllers
{
    public class NotesController : Controller
    {
        private readonly ILogger<NotesController> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public NotesController(ILogger<NotesController> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetNotes()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                var notes = await dbContext.Notes
                    .AsNoTracking()
                    .OrderBy(x => x.Name)
                    .Select(x => new
                    {
                        id = x.Id,
                        text = $"{x.Name})"
                    })
                    .ToListAsync();

                return Json(notes);
            }
        }

        [HttpPost("CreateNote")]
        public async Task<IActionResult> CreateNote([FromBody] Note model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("Ошибка данных");

                int currentYear = DateTime.Now.Year;

                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                    // Get Max Number for this Year
                    int? maxNumber = await dbContext.Notes
                        .AsNoTracking()
                        .Where(n => n.DateCreate.Year == currentYear && !n.IsDeleted)
                        .MaxAsync(n => (int?)n.Number); 

                    int resultMaxNum = maxNumber ?? 0; 

                    model.Number = resultMaxNum + 1;
                    dbContext.Notes.Add(model);
                    await dbContext.SaveChangesAsync();

                    // Get list of Notes
                    var notes = await dbContext.Notes
                        .AsNoTracking()
                        .OrderByDescending(n => n.DateCreate)
                        .Select(n => new
                        {
                            id = n.Id,
                            text = $"{n.GetNumString()} - {n.Name}"
                        })
                        .ToListAsync();

                    return Json(new
                    {
                        success = true,
                        notes = notes,
                        newNoteId = model.Id
                    });
                }
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex.Message);
                return Json(null);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNoteInfo(Guid id)
        {
            string notFoundMessage = "Не удалось получить информацию";
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                    var note = await dbContext.Notes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                    if (note == null)
                    {
                        //return NotFound();
                        return Json(new
                        {
                            customer = notFoundMessage,
                            description = notFoundMessage
                        });
                    }

                    return Json(new
                    {
                        customer = note.Customer,
                        description = note.Description
                    });
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(null);

            }
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}

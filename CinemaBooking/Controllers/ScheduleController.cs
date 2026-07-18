using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaBooking.Data;
using CinemaBooking.Models;
using CinemaBooking.ViewModels;

public class ScheduleController : Controller
{
    private readonly ApplicationDbContext _context;

    public ScheduleController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Schedule
    public async Task<IActionResult> Index()
    {
        var schedules = await _context.Schedules
            .Include(s => s.Movie)
            .Include(s => s.Cinema)
            .ToListAsync();

        return View(schedules);
    }

    // GET: Schedule/Details/5
    public async Task<IActionResult> Details(int? scheduleid)
    {
        if (scheduleid == null)
        {
            return NotFound();
        }

        var schedule = await _context.Schedules
            .Include(s => s.Movie)
            .Include(s => s.Cinema)
            .FirstOrDefaultAsync(s => s.ScheduleId == scheduleid);

        if (schedule == null)
        {
            return NotFound();
        }

        return View(schedule);
    }

    // GET: Schedule/Create
    public async Task<IActionResult> Create()
    {
        var viewModel = new ScheduleCreateViewModel();

        await LoadCreateDropdownsAsync(viewModel);

        return View(viewModel);
    }

    // POST: Schedule/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ScheduleCreateViewModel viewModel)
    {
        viewModel.SelectedPeriods ??= new List<ShowPeriod>();

        var movie = await _context.Movies
            .FirstOrDefaultAsync(m => m.MovieId == viewModel.MovieId);

        if (movie == null)
        {
            ModelState.AddModelError(
                nameof(viewModel.MovieId),
                "ไม่พบข้อมูลภาพยนตร์"
            );
        }

        var cinemaExists = await _context.Cinemas
            .AnyAsync(c => c.CinemaId == viewModel.CinemaId);

        if (!cinemaExists)
        {
            ModelState.AddModelError(
                nameof(viewModel.CinemaId),
                "ไม่พบข้อมูลโรงภาพยนตร์"
            );
        }

        if (viewModel.SelectedPeriods.Count == 0)
        {
            ModelState.AddModelError(
                nameof(viewModel.SelectedPeriods),
                "กรุณาเลือกรอบฉายอย่างน้อย 1 รอบ"
            );
        }

        if (movie != null)
        {
            var showDate = viewModel.ShowDate.Date;
            var startDate = movie.StartDate.Date;
            var stopDate = movie.StopDate.Date;

            if (showDate < startDate || showDate > stopDate)
            {
                ModelState.AddModelError(
                    nameof(viewModel.ShowDate),
                    $"วันที่ฉายต้องอยู่ระหว่าง " +
                    $"{startDate:dd/MM/yyyy} - {stopDate:dd/MM/yyyy}"
                );
            }
        }

        // ป้องกันค่าซ้ำที่ส่งมาจากหน้าเว็บ
        viewModel.SelectedPeriods = viewModel.SelectedPeriods
            .Distinct()
            .ToList();

        var existingPeriods = await _context.Schedules
            .Where(s =>
                s.MovieId == viewModel.MovieId &&
                s.CinemaId == viewModel.CinemaId &&
                s.ShowDate.Date == viewModel.ShowDate.Date)
            .Select(s => s.ShowPeriod)
            .ToListAsync();

        var duplicatedPeriods = viewModel.SelectedPeriods
            .Where(period => existingPeriods.Contains(period))
            .ToList();

        if (duplicatedPeriods.Count > 0)
        {
            var periodNames = duplicatedPeriods
                .Select(GetShowPeriodName);

            ModelState.AddModelError(
                string.Empty,
                $"รอบฉายนี้มีอยู่แล้ว: {string.Join(", ", periodNames)}"
            );
        }

        var totalPeriods =
            existingPeriods.Count + viewModel.SelectedPeriods.Count;

        if (totalPeriods > 3)
        {
            ModelState.AddModelError(
                string.Empty,
                $"เพิ่มรอบฉายไม่ได้ เนื่องจากวันดังกล่าวจะมีทั้งหมด " +
                $"{totalPeriods} รอบ ซึ่งเกินจำนวนสูงสุด 3 รอบต่อวัน"
            );
        }

        if (!ModelState.IsValid)
        {
            await LoadCreateDropdownsAsync(viewModel);
            return View(viewModel);
        }

        foreach (var period in viewModel.SelectedPeriods)
        {
            _context.Schedules.Add(new Schedule
            {
                MovieId = viewModel.MovieId,
                CinemaId = viewModel.CinemaId,
                ShowDate = viewModel.ShowDate.Date,
                ShowPeriod = period
            });
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "เพิ่มรอบฉายเรียบร้อยแล้ว";

        return RedirectToAction(nameof(Index));
    }

    // GET: Schedule/Edit/5
    public async Task<IActionResult> Edit(int? scheduleid)
    {
        if (scheduleid == null)
        {
            return NotFound();
        }

        var schedule = await _context.Schedules.FindAsync(scheduleid);

        if (schedule == null)
        {
            return NotFound();
        }

        await LoadDropdowns();

        return View(schedule);
    }

    // POST: Schedule/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int scheduleid,
        [Bind("ScheduleId,MovieId,CinemaId,ShowDate,ShowPeriod")]
        Schedule schedule)
    {
        if (scheduleid != schedule.ScheduleId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(schedule);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScheduleExists(schedule.ScheduleId))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        await LoadDropdowns();

        return View(schedule);
    }

    // GET: Schedule/Delete/5
    public async Task<IActionResult> Delete(int? scheduleid)
    {
        if (scheduleid == null)
        {
            return NotFound();
        }

        var schedule = await _context.Schedules
            .Include(s => s.Movie)
            .Include(s => s.Cinema)
            .FirstOrDefaultAsync(s => s.ScheduleId == scheduleid);

        if (schedule == null)
        {
            return NotFound();
        }

        return View(schedule);
    }

    // POST: Schedule/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int scheduleid)
    {
        var schedule = await _context.Schedules.FindAsync(scheduleid);

        if (schedule != null)
        {
            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadDropdowns()
    {
        ViewData["MovieId"] = new SelectList(
            await _context.Movies.ToListAsync(),
            "MovieId",
            "MovieName"
        );

        ViewData["CinemaId"] = new SelectList(
            await _context.Cinemas.ToListAsync(),
            "CinemaId",
            "CinemaName"
        );
    }

    private bool ScheduleExists(int scheduleid)
    {
        return _context.Schedules.Any(s => s.ScheduleId == scheduleid);
    }

    private async Task LoadCreateDropdownsAsync(
    ScheduleCreateViewModel viewModel)
    {
        viewModel.Movies = await _context.Movies
            .OrderBy(m => m.MovieName)
            .Select(m => new SelectListItem
            {
                Value = m.MovieId.ToString(),
                Text = m.MovieName,
                Selected = m.MovieId == viewModel.MovieId
            })
            .ToListAsync();

        viewModel.Cinemas = await _context.Cinemas
            .OrderBy(c => c.CinemaName)
            .Select(c => new SelectListItem
            {
                Value = c.CinemaId.ToString(),
                Text = c.CinemaName,
                Selected = c.CinemaId == viewModel.CinemaId
            })
            .ToListAsync();
    }

    private static string GetShowPeriodName(ShowPeriod period)
    {
        return period switch
        {
            ShowPeriod.Morning => "รอบเช้า",
            ShowPeriod.Afternoon => "รอบกลางวัน",
            ShowPeriod.Evening => "รอบเย็น",
            ShowPeriod.Night => "รอบดึก",
            _ => "-"
        };
    }
}
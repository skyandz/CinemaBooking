using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaBooking.Data;
using CinemaBooking.Models;

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
        await LoadDropdowns();

        return View();
    }

    // POST: Schedule/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("MovieId,CinemaId,ShowDate,ShowTime")] Schedule schedule)
    {
        if (ModelState.IsValid)
        {
            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        await LoadDropdowns();

        return View(schedule);
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
        [Bind("ScheduleId,MovieId,CinemaId,ShowDate,ShowTime")]
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
}
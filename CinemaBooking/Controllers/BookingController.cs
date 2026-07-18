using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaBooking.Data;
using CinemaBooking.Models;

public class BookingController : Controller
{
    private readonly ApplicationDbContext _context;

    public BookingController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Booking
    public async Task<IActionResult> Index()
    {
        var bookings = await _context.Bookings
            .Include(b => b.BookingDetails)
                .ThenInclude(bd => bd.Schedule)
                    .ThenInclude(s => s.Movie)
            .Include(b => b.BookingDetails)
                .ThenInclude(bd => bd.Schedule)
                    .ThenInclude(s => s.Cinema)
            .ToListAsync();

        return View(bookings);
    }

    // GET: Booking/Details/5
    public async Task<IActionResult> Details(int? bookingid)
    {
        if (bookingid == null)
        {
            return NotFound();
        }

        var booking = await _context.Bookings
            .Include(b => b.BookingDetails)
                .ThenInclude(bd => bd.Schedule)
                    .ThenInclude(s => s.Movie)
            .Include(b => b.BookingDetails)
                .ThenInclude(bd => bd.Schedule)
                    .ThenInclude(s => s.Cinema)
            .FirstOrDefaultAsync(b => b.BookingId == bookingid);

        if (booking == null)
        {
            return NotFound();
        }

        return View(booking);
    }

    // GET: Booking/Create
    public async Task<IActionResult> Create()
    {
        var schedules = await _context.Schedules
            .Include(s => s.Movie)
            .Include(s => s.Cinema)
            .ToListAsync();

        ViewBag.Schedules = schedules;

        return View();
    }

    // POST: Booking/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string phoneNumber,
        int scheduleId)
    {
        // ตรวจสอบว่า Schedule มีอยู่จริงหรือไม่
        var schedule = await _context.Schedules
            .Include(s => s.Movie)
            .Include(s => s.Cinema)
            .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);

        if (schedule == null)
        {
            return NotFound();
        }

        return RedirectToAction(
            nameof(SelectSeat),
            new
            {
                phoneNumber = phoneNumber,
                scheduleId = scheduleId
            }
        );
    }

    // GET: Booking/SelectSeat
    public async Task<IActionResult> SelectSeat(
        string phoneNumber,
        int scheduleId)
    {
        var schedule = await _context.Schedules
            .Include(s => s.Movie)
            .Include(s => s.Cinema)
            .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);

        if (schedule == null)
        {
            return NotFound();
        }

        var bookedSeats = await _context.BookingDetails
            .Where(bd => bd.ScheduleId == scheduleId)
            .Select(bd => bd.SeatNo)
            .ToListAsync();

        ViewBag.PhoneNumber = phoneNumber;
        ViewBag.BookedSeats = bookedSeats;

        return View(schedule);
    }

    // POST: Booking/Confirm
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(
        string phoneNumber,
        int scheduleId,
        List<string> selectedSeats)
    {
        if (selectedSeats == null ||
            selectedSeats.Count == 0)
        {
            return BadRequest(
                "Please select at least one seat."
            );
        }

        var schedule = await _context.Schedules
            .FirstOrDefaultAsync(
                s => s.ScheduleId == scheduleId
            );

        if (schedule == null)
        {
            return NotFound();
        }

        // ตรวจสอบว่าที่นั่งถูกจองไปแล้วหรือไม่
        var alreadyBookedSeats =
            await _context.BookingDetails
                .Where(bd =>
                    bd.ScheduleId == scheduleId &&
                    selectedSeats.Contains(bd.SeatNo))
                .Select(bd => bd.SeatNo)
                .ToListAsync();

        if (alreadyBookedSeats.Any())
        {
            return BadRequest(
                "Some seats have already been booked."
            );
        }

        // สร้าง Booking
        var booking = new Booking
        {
            PhoneNumber = phoneNumber,
            BookingDate = DateTime.Now
        };

        // สร้าง BookingDetail แต่ละที่นั่ง
        foreach (var seat in selectedSeats)
        {
            booking.BookingDetails.Add(
                new BookingDetail
                {
                    ScheduleId = scheduleId,
                    SeatNo = seat
                }
            );
        }

        _context.Bookings.Add(booking);

        await _context.SaveChangesAsync();

        return RedirectToAction(
            nameof(Details),
            new
            {
                bookingid = booking.BookingId
            }
        );
    }
}
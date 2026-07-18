using CinemaBooking.Data;
using CinemaBooking.Models;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class BookingController : Controller
{
    private readonly ApplicationDbContext _context;

    public BookingController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Booking
    // ตัวอย่าง URL:
    // /Booking
    // /Booking?movieId=1
    // /Booking?movieId=1&scheduleId=3
    public async Task<IActionResult> Index(
        int? movieId,
        int? scheduleId)
    {
        var viewModel = new BookingPageViewModel
        {
            MovieId = movieId,
            ScheduleId = scheduleId
        };

        await LoadMoviesAsync(viewModel);
        await LoadSchedulesAsync(viewModel);

        // ถ้ายังไม่ได้เลือกรอบฉาย จะแสดงเฉพาะ Dropdown
        if (scheduleId == null)
        {
            return View(viewModel);
        }

        var schedule = await _context.Schedules
            .Include(s => s.Movie)
            .Include(s => s.Cinema)
            .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);

        if (schedule == null)
        {
            return NotFound();
        }

        // ป้องกันกรณีส่ง MovieId ที่ไม่ตรงกับ Schedule
        viewModel.MovieId = schedule.MovieId;
        viewModel.ScheduleId = schedule.ScheduleId;

        viewModel.MovieName = schedule.Movie.MovieName;
        viewModel.CinemaName = schedule.Cinema.CinemaName;
        viewModel.ShowDate = schedule.ShowDate;
        viewModel.ShowPeriod = schedule.ShowPeriod;

        viewModel.RowCount = schedule.Cinema.RowCount;
        viewModel.ColumnCount = schedule.Cinema.ColumnCount;

        await LoadMoviesAsync(viewModel);
        await LoadSchedulesAsync(viewModel);
        await LoadSeatsAsync(viewModel);

        return View(viewModel);
    }

    // POST: Booking/Book
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(
        int movieId,
        int scheduleId,
        string phoneNumber,
        List<string>? selectedSeats)
    {
        phoneNumber = phoneNumber?.Trim() ?? string.Empty;
        selectedSeats ??= new List<string>();

        // ป้องกันค่าซ้ำ เช่น A1 ถูกส่งมาสองครั้ง
        selectedSeats = selectedSeats
            .Where(seat => !string.IsNullOrWhiteSpace(seat))
            .Distinct()
            .ToList();

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            TempData["ErrorMessage"] =
                "กรุณากรอกเบอร์โทรศัพท์";

            return RedirectToAction(nameof(Index), new
            {
                movieId,
                scheduleId
            });
        }

        if (selectedSeats.Count == 0)
        {
            TempData["ErrorMessage"] =
                "กรุณาเลือกที่นั่งอย่างน้อย 1 ที่นั่ง";

            return RedirectToAction(nameof(Index), new
            {
                movieId,
                scheduleId
            });
        }

        if (selectedSeats.Count > 4)
        {
            TempData["ErrorMessage"] =
                "สามารถจองได้ครั้งละไม่เกิน 4 ที่นั่ง";

            return RedirectToAction(nameof(Index), new
            {
                movieId,
                scheduleId
            });
        }

        var schedule = await _context.Schedules
            .Include(s => s.Cinema)
            .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);

        if (schedule == null)
        {
            return NotFound();
        }

        // ตรวจสอบว่าหมายเลขที่นั่งที่ส่งมาอยู่ในผังจริงหรือไม่
        var validSeats = GenerateValidSeatNumbers(
            schedule.Cinema.RowCount,
            schedule.Cinema.ColumnCount
        );

        var invalidSeats = selectedSeats
            .Where(seat => !validSeats.Contains(seat))
            .ToList();

        if (invalidSeats.Count > 0)
        {
            TempData["ErrorMessage"] =
                "พบหมายเลขที่นั่งไม่ถูกต้อง";

            return RedirectToAction(nameof(Index), new
            {
                movieId,
                scheduleId
            });
        }

        // ตรวจสอบว่ามีคนจองที่นั่งไปก่อนแล้วหรือไม่
        var alreadyBookedSeats = await _context.BookingDetails
            .Where(bd =>
                bd.ScheduleId == scheduleId &&
                selectedSeats.Contains(bd.SeatNo))
            .Select(bd => bd.SeatNo)
            .ToListAsync();

        if (alreadyBookedSeats.Count > 0)
        {
            TempData["ErrorMessage"] =
                $"ที่นั่ง {string.Join(", ", alreadyBookedSeats)} ถูกจองแล้ว";

            return RedirectToAction(nameof(Index), new
            {
                movieId,
                scheduleId
            });
        }

        var booking = new Booking
        {
            PhoneNumber = phoneNumber,
            BookingDate = DateTime.Now
        };

        foreach (var seatNo in selectedSeats)
        {
            booking.BookingDetails.Add(new BookingDetail
            {
                ScheduleId = scheduleId,
                SeatNo = seatNo
            });
        }

        _context.Bookings.Add(booking);

        try
        {
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"จองที่นั่ง {string.Join(", ", selectedSeats)} สำเร็จ";
        }
        catch (DbUpdateException)
        {
            // รองรับกรณีมีคนอื่นกดจองที่นั่งเดียวกันในเวลาใกล้กัน
            TempData["ErrorMessage"] =
                "ไม่สามารถจองที่นั่งได้ เนื่องจากบางที่นั่งอาจถูกจองไปแล้ว";
        }

        return RedirectToAction(nameof(Index), new
        {
            movieId,
            scheduleId
        });
    }

    // POST: Booking/Cancel
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(
        int movieId,
        int scheduleId,
        string phoneNumber,
        List<string>? selectedSeats)
    {
        phoneNumber = phoneNumber?.Trim() ?? string.Empty;
        selectedSeats ??= new List<string>();

        selectedSeats = selectedSeats
            .Where(seat => !string.IsNullOrWhiteSpace(seat))
            .Distinct()
            .ToList();

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            TempData["ErrorMessage"] =
                "กรุณากรอกเบอร์โทรศัพท์ที่ใช้จอง";

            return RedirectToAction(nameof(Index), new
            {
                movieId,
                scheduleId
            });
        }

        if (selectedSeats.Count == 0)
        {
            TempData["ErrorMessage"] =
                "กรุณาเลือกที่นั่งที่ต้องการยกเลิก";

            return RedirectToAction(nameof(Index), new
            {
                movieId,
                scheduleId
            });
        }

        // ค้นหาเฉพาะที่นั่งของรอบฉายและเบอร์โทรนี้
        var bookingDetails = await _context.BookingDetails
            .Include(bd => bd.Booking)
            .Where(bd =>
                bd.ScheduleId == scheduleId &&
                selectedSeats.Contains(bd.SeatNo) &&
                bd.Booking.PhoneNumber == phoneNumber)
            .ToListAsync();

        if (bookingDetails.Count == 0)
        {
            TempData["ErrorMessage"] =
                "ไม่พบการจองที่ตรงกับเบอร์โทรศัพท์และที่นั่งที่เลือก";

            return RedirectToAction(nameof(Index), new
            {
                movieId,
                scheduleId
            });
        }

        var cancelledSeats = bookingDetails
            .Select(bd => bd.SeatNo)
            .ToList();

        var affectedBookingIds = bookingDetails
            .Select(bd => bd.BookingId)
            .Distinct()
            .ToList();

        _context.BookingDetails.RemoveRange(bookingDetails);
        await _context.SaveChangesAsync();

        // ถ้า Booking ไม่มีที่นั่งเหลือแล้ว ให้ลบ Booking หลักด้วย
        var emptyBookings = await _context.Bookings
            .Where(b =>
                affectedBookingIds.Contains(b.BookingId) &&
                !b.BookingDetails.Any())
            .ToListAsync();

        if (emptyBookings.Count > 0)
        {
            _context.Bookings.RemoveRange(emptyBookings);
            await _context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] =
            $"ยกเลิกที่นั่ง {string.Join(", ", cancelledSeats)} สำเร็จ";

        return RedirectToAction(nameof(Index), new
        {
            movieId,
            scheduleId
        });
    }

    private async Task LoadMoviesAsync(
        BookingPageViewModel viewModel)
    {
        viewModel.Movies = await _context.Movies
            .OrderBy(m => m.MovieName)
            .Select(m => new SelectListItem
            {
                Value = m.MovieId.ToString(),
                Text = m.MovieName,
                Selected = viewModel.MovieId == m.MovieId
            })
            .ToListAsync();
    }

    private async Task LoadSchedulesAsync(
        BookingPageViewModel viewModel)
    {
        viewModel.Schedules = new List<SelectListItem>();

        if (viewModel.MovieId == null)
        {
            return;
        }

        viewModel.Schedules = await _context.Schedules
            .Where(s => s.MovieId == viewModel.MovieId)
            .OrderBy(s => s.ShowDate)
            .ThenBy(s => s.ShowPeriod)
            .Select(s => new SelectListItem
            {
                Value = s.ScheduleId.ToString(),

                Text =
                    $"{s.ShowDate:dd/MM/yyyy} - " +
                    $"{GetShowPeriodName(s.ShowPeriod)} - " +
                    $"{s.Cinema.CinemaName}",

                Selected =
                    viewModel.ScheduleId == s.ScheduleId
            })
            .ToListAsync();
    }

    private async Task LoadSeatsAsync(
        BookingPageViewModel viewModel)
    {
        if (viewModel.ScheduleId == null)
        {
            return;
        }

        var bookedSeatData = await _context.BookingDetails
            .Where(bd =>
                bd.ScheduleId == viewModel.ScheduleId)
            .Select(bd => new
            {
                bd.SeatNo,
                bd.Booking.PhoneNumber
            })
            .ToListAsync();

        viewModel.Seats.Clear();

        // Prototype กำหนด:
        // ตัวอักษร = คอลัมน์
        // ตัวเลข = แถว
        //
        // A1 B1 C1
        // A2 B2 C2
        for (var row = 1; row <= viewModel.RowCount; row++)
        {
            for (var column = 0;
                 column < viewModel.ColumnCount;
                 column++)
            {
                var columnLetter =
                    (char)('A' + column);

                var seatNo =
                    $"{columnLetter}{row}";

                var bookingData =
                    bookedSeatData.FirstOrDefault(
                        seat => seat.SeatNo == seatNo
                    );

                viewModel.Seats.Add(new SeatViewModel
                {
                    SeatNo = seatNo,
                    IsBooked = bookingData != null,
                    PhoneNumber = bookingData?.PhoneNumber
                });
            }
        }
    }

    private static HashSet<string> GenerateValidSeatNumbers(
        int rowCount,
        int columnCount)
    {
        var seats = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase
        );

        for (var row = 1; row <= rowCount; row++)
        {
            for (var column = 0;
                 column < columnCount;
                 column++)
            {
                var columnLetter =
                    (char)('A' + column);

                seats.Add($"{columnLetter}{row}");
            }
        }

        return seats;
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
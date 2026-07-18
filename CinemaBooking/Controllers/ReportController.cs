using CinemaBooking.Data;
using CinemaBooking.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var topMovies = await _context.BookingDetails
                .AsNoTracking()
                .GroupBy(bookingDetail => new
                {
                    bookingDetail.Schedule.MovieId,
                    bookingDetail.Schedule.Movie.MovieName
                })
                .Select(group => new MovieBookingReport
                {
                    MovieName = group.Key.MovieName,
                    TicketCount = group.Count()
                })
                .OrderByDescending(movie => movie.TicketCount)
                .Take(5)
                .ToListAsync();

            var viewModel = new ReportViewModel
            {
                TotalBookings = await _context.Bookings
                    .AsNoTracking()
                    .CountAsync(),

                TotalTickets = await _context.BookingDetails
                    .AsNoTracking()
                    .CountAsync(),

                TotalMovies = await _context.Movies
                    .AsNoTracking()
                    .CountAsync(),

                TotalCinemas = await _context.Cinemas
                    .AsNoTracking()
                    .CountAsync(),

                TotalSchedules = await _context.Schedules
                    .AsNoTracking()
                    .CountAsync(),

                LastUpdated = DateTime.Now,

                TopMovies = topMovies
            };

            return View(viewModel);
        }
    }
}
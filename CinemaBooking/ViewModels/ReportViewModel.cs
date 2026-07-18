namespace CinemaBooking.Models.ViewModels
{
    public class ReportViewModel
    {
        public int TotalBookings { get; set; }

        public int TotalTickets { get; set; }

        public int TotalMovies { get; set; }

        public int TotalCinemas { get; set; }

        public int TotalSchedules { get; set; }

        public DateTime LastUpdated { get; set; }

        public List<MovieBookingReport> TopMovies { get; set; }
            = new List<MovieBookingReport>();
    }

    public class MovieBookingReport
    {
        public string MovieName { get; set; } = string.Empty;

        public int TicketCount { get; set; }
    }
}
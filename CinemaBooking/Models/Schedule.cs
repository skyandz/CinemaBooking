using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CinemaBooking.Models;

public class Schedule
{
    public int ScheduleId { get; set; }

    public int MovieId { get; set; }

    public int CinemaId { get; set; }

    public DateTime ShowDate { get; set; }

    public TimeSpan ShowTime { get; set; }

    [ValidateNever]
    public Movie Movie { get; set; } = null!;

    [ValidateNever]
    public Cinema Cinema { get; set; } = null!;

    [ValidateNever]
    public ICollection<BookingDetail> BookingDetails { get; set; }
        = new List<BookingDetail>();
}
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Models;

public class Schedule
{
    public int ScheduleId { get; set; }

    [Required]
    public int MovieId { get; set; }

    [Required]
    public int CinemaId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime ShowDate { get; set; }

    [Required]
    public ShowPeriod ShowPeriod { get; set; }

    [ValidateNever]
    public Movie Movie { get; set; } = null!;

    [ValidateNever]
    public Cinema Cinema { get; set; } = null!;

    [ValidateNever]
    public ICollection<BookingDetail> BookingDetails { get; set; }
        = new List<BookingDetail>();
}
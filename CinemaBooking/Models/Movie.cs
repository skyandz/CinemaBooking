namespace CinemaBooking.Models;

using System.ComponentModel.DataAnnotations;

public class Movie
{
    public int MovieId { get; set; }

    public string MovieName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime StopDate { get; set; }

    // Movie 1 เรื่อง มีได้หลายรอบฉาย
    public ICollection<Schedule> Schedules { get; set; }
        = new List<Schedule>();
}
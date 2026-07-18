namespace CinemaBooking.Models;

public class Movie
{
    public int MovieId { get; set; }

    public string MovieName { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime StopDate { get; set; }

    // Movie 1 เรื่อง มีได้หลายรอบฉาย
    public ICollection<Schedule> Schedules { get; set; }
        = new List<Schedule>();
}
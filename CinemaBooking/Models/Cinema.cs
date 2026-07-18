namespace CinemaBooking.Models;

public class Cinema
{
    public int CinemaId { get; set; }

    public string CinemaName { get; set; } = string.Empty;

    public int RowCount { get; set; }

    public int ColumnCount { get; set; }

    public ICollection<Schedule> Schedules { get; set; }
        = new List<Schedule>();
}
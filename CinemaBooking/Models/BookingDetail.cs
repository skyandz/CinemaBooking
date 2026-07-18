namespace CinemaBooking.Models;

public class BookingDetail
{
    public int BookingDetailId { get; set; }

    public int BookingId { get; set; }

    public int ScheduleId { get; set; }

    public string SeatNo { get; set; } = string.Empty;

    public Booking Booking { get; set; } = null!;

    public Schedule Schedule { get; set; } = null!;
}
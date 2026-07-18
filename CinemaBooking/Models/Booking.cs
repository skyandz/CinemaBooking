namespace CinemaBooking.Models;

public class Booking
{
    public int BookingId { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime BookingDate { get; set; }

    public ICollection<BookingDetail> BookingDetails { get; set; }
        = new List<BookingDetail>();
}
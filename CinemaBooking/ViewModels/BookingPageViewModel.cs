using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.ViewModels;

public class BookingPageViewModel
{
    // ค่าที่เลือกจาก Dropdown
    public int? MovieId { get; set; }

    public int? ScheduleId { get; set; }

    // ใช้สร้าง Dropdown
    public List<SelectListItem> Movies { get; set; } = new();

    public List<SelectListItem> Schedules { get; set; } = new();

    // ข้อมูลรอบฉายที่เลือก
    public string? MovieName { get; set; }

    public string? CinemaName { get; set; }

    public DateTime? ShowDate { get; set; }

    public TimeSpan? ShowTime { get; set; }

    // ขนาดผังที่นั่ง
    public int RowCount { get; set; }

    public int ColumnCount { get; set; }

    // รายการที่นั่งทั้งหมด
    public List<SeatViewModel> Seats { get; set; } = new();

    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    // ที่นั่งที่ผู้ใช้เลือก
    public List<string> SelectedSeats { get; set; } = new();
}

public class SeatViewModel
{
    public string SeatNo { get; set; } = string.Empty;

    public bool IsBooked { get; set; }

    // ใช้แสดงเบอร์โทรบนที่นั่งที่จองแล้ว
    public string? PhoneNumber { get; set; }
}
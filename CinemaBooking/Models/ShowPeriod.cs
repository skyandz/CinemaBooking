using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Models;

public enum ShowPeriod
{
    [Display(Name = "รอบเช้า")]
    Morning = 1,

    [Display(Name = "รอบกลางวัน")]
    Afternoon = 2,

    [Display(Name = "รอบเย็น")]
    Evening = 3,

    [Display(Name = "รอบดึก")]
    Night = 4
}
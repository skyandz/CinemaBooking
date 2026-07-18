using CinemaBooking.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.ViewModels;

public class ScheduleCreateViewModel
{
    [Required]
    public int MovieId { get; set; }

    [Required]
    public int CinemaId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime ShowDate { get; set; } = DateTime.Today;

    public List<ShowPeriod> SelectedPeriods { get; set; } = new();

    public List<SelectListItem> Movies { get; set; } = new();

    public List<SelectListItem> Cinemas { get; set; } = new();
}
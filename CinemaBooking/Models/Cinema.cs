using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CinemaBooking.Models;

public class Cinema
{
    public int CinemaId { get; set; }

    [Required]
    public string CinemaName { get; set; } = string.Empty;

    [Range(1, 26)]
    public int RowCount { get; set; }

    [Range(1, 50)]
    public int ColumnCount { get; set; }

    [ValidateNever]
    public ICollection<Schedule> Schedules { get; set; }
        = new List<Schedule>();
}
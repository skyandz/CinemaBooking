using CinemaBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Movie> Movies => Set<Movie>();

    public DbSet<Cinema> Cinemas => Set<Cinema>();

    public DbSet<Schedule> Schedules => Set<Schedule>();

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<BookingDetail> BookingDetails => Set<BookingDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BookingDetail>()
            .HasIndex(b => new
            {
                b.ScheduleId,
                b.SeatNo
            })
            .IsUnique();
    }
}


using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Booking.Infrastructure;

/// <summary>
/// Booking-owned DbContext. Owns PostgreSQL schema <c>booking</c>.
/// T002: Booking aggregate only; no passengers/holds/payments.
/// </summary>
public sealed class BookingDbContext : DbContext
{
    public const string SchemaName = "booking";

    public BookingDbContext(DbContextOptions<BookingDbContext> options)
        : base(options)
    {
    }

    public DbSet<TravelCore.Modules.Booking.Domain.Booking> Bookings => Set<TravelCore.Modules.Booking.Domain.Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);
    }
}

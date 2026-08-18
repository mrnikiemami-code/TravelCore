using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Booking.Infrastructure;

/// <summary>
/// Booking-owned DbContext. Owns PostgreSQL schema <c>booking</c>.
/// T001: no product entity sets.
/// </summary>
public sealed class BookingDbContext : DbContext
{
    public const string SchemaName = "booking";

    public BookingDbContext(DbContextOptions<BookingDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);
    }
}

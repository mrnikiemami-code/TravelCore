using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Booking.Infrastructure;

/// <summary>
/// Booking-owned DbContext. Owns PostgreSQL schema <c>booking</c>.
/// T003: Booking aggregate + capacity holds; no passengers/payments.
/// </summary>
public sealed class BookingDbContext : DbContext
{
    public const string SchemaName = "booking";

    public BookingDbContext(DbContextOptions<BookingDbContext> options)
        : base(options)
    {
    }

    public DbSet<TravelCore.Modules.Booking.Domain.Booking> Bookings => Set<TravelCore.Modules.Booking.Domain.Booking>();

    public DbSet<TravelCore.Modules.Booking.Domain.CapacityHold> CapacityHolds =>
        Set<TravelCore.Modules.Booking.Domain.CapacityHold>();

    public DbSet<TravelCore.Modules.Booking.Domain.DepartureCapacityAccount> DepartureCapacityAccounts =>
        Set<TravelCore.Modules.Booking.Domain.DepartureCapacityAccount>();

    public DbSet<TravelCore.Modules.Booking.Domain.BookingAccessCredential> AccessCredentials =>
        Set<TravelCore.Modules.Booking.Domain.BookingAccessCredential>();

    public DbSet<TravelCore.Modules.Booking.Domain.BookingPublicIdempotencyRecord> PublicIdempotency =>
        Set<TravelCore.Modules.Booking.Domain.BookingPublicIdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);
    }
}

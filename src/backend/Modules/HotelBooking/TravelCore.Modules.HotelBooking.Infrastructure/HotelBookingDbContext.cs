using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.HotelBooking.Infrastructure;

/// <summary>
/// HotelBooking-owned DbContext. Owns PostgreSQL schema <c>hotel_booking</c>.
/// No product tables in T001 (P21-R1 scaffolding only).
/// </summary>
public sealed class HotelBookingDbContext : DbContext
{
    public const string SchemaName = "hotel_booking";

    public HotelBookingDbContext(DbContextOptions<HotelBookingDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelBookingDbContext).Assembly);
    }
}

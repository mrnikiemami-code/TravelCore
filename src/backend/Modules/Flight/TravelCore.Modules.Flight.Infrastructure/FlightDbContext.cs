using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Flight.Domain;
using FlightBookingAggregate = TravelCore.Modules.Flight.Domain.FlightBooking;

namespace TravelCore.Modules.Flight.Infrastructure;

/// <summary>
/// Flight-owned DbContext. Owns PostgreSQL schema <c>flight</c>.
/// T002 persists FlightBooking itinerary/passengers. Same-schema FKs only.
/// </summary>
public sealed class FlightDbContext : DbContext
{
    public const string SchemaName = "flight";

    public FlightDbContext(DbContextOptions<FlightDbContext> options)
        : base(options)
    {
    }

    public DbSet<FlightBookingAggregate> FlightBookings => Set<FlightBookingAggregate>();

    public DbSet<FlightJourney> FlightJourneys => Set<FlightJourney>();

    public DbSet<FlightSegment> FlightSegments => Set<FlightSegment>();

    public DbSet<FlightPassenger> FlightPassengers => Set<FlightPassenger>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FlightDbContext).Assembly);
    }
}

using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Flight.Domain;
using FlightBookingAggregate = TravelCore.Modules.Flight.Domain.FlightBooking;

namespace TravelCore.Modules.Flight.Infrastructure;

/// <summary>
/// Flight-owned DbContext. Owns PostgreSQL schema <c>flight</c>.
/// T004 persists itinerary plus immutable offer/monetary/fare-rule snapshots. Same-schema FKs only.
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

    public DbSet<FlightOfferSnapshot> FlightOfferSnapshots => Set<FlightOfferSnapshot>();

    public DbSet<FlightBookingMonetarySnapshot> FlightBookingMonetarySnapshots =>
        Set<FlightBookingMonetarySnapshot>();

    public DbSet<FlightPassengerCategoryFareSnapshot> FlightPassengerCategoryFareSnapshots =>
        Set<FlightPassengerCategoryFareSnapshot>();

    public DbSet<FlightFareRulesSnapshot> FlightFareRuleSnapshots => Set<FlightFareRulesSnapshot>();

    public DbSet<FlightBaggageAllowanceSnapshot> FlightBaggageAllowanceSnapshots =>
        Set<FlightBaggageAllowanceSnapshot>();

    public DbSet<FlightOfferIdempotencyRecord> FlightOfferIdempotency => Set<FlightOfferIdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FlightDbContext).Assembly);
    }
}

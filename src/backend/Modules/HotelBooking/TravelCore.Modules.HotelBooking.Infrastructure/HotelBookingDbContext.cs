using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.HotelBooking.Domain;
using HotelBookingAggregate = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Modules.HotelBooking.Infrastructure;

/// <summary>
/// HotelBooking-owned DbContext. Owns PostgreSQL schema <c>hotel_booking</c>.
/// T002 persist stay/rooms/guests. Same-schema FKs only.
/// </summary>
public sealed class HotelBookingDbContext : DbContext
{
    public const string SchemaName = "hotel_booking";

    public HotelBookingDbContext(DbContextOptions<HotelBookingDbContext> options)
        : base(options)
    {
    }

    public DbSet<HotelBookingAggregate> HotelBookings => Set<HotelBookingAggregate>();

    public DbSet<RoomReservation> RoomReservations => Set<RoomReservation>();

    public DbSet<HotelBookingGuest> HotelBookingGuests => Set<HotelBookingGuest>();

    public DbSet<HotelAvailabilityHold> HotelAvailabilityHolds => Set<HotelAvailabilityHold>();

    public DbSet<HotelAvailabilityHoldRoom> HotelAvailabilityHoldRooms => Set<HotelAvailabilityHoldRoom>();

    public DbSet<HotelAvailabilityHoldIdempotencyRecord> HotelAvailabilityHoldIdempotency =>
        Set<HotelAvailabilityHoldIdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelBookingDbContext).Assembly);
    }
}

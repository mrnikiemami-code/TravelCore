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

    public DbSet<HotelRateOfferSnapshot> HotelRateOfferSnapshots => Set<HotelRateOfferSnapshot>();

    public DbSet<HotelRoomRateSnapshot> HotelRoomRateSnapshots => Set<HotelRoomRateSnapshot>();

    public DbSet<HotelBookingMonetarySnapshot> HotelBookingMonetarySnapshots => Set<HotelBookingMonetarySnapshot>();

    public DbSet<HotelChargeComponentSnapshot> HotelChargeComponentSnapshots => Set<HotelChargeComponentSnapshot>();

    public DbSet<HotelCancellationPolicySnapshot> HotelCancellationPolicySnapshots =>
        Set<HotelCancellationPolicySnapshot>();

    public DbSet<HotelCancellationPenaltyRule> HotelCancellationPenaltyRules => Set<HotelCancellationPenaltyRule>();

    public DbSet<HotelRateOfferIdempotencyRecord> HotelRateOfferIdempotency =>
        Set<HotelRateOfferIdempotencyRecord>();

    public DbSet<HotelSupplierReservation> HotelSupplierReservations => Set<HotelSupplierReservation>();

    public DbSet<HotelSupplierReservationAttempt> HotelSupplierReservationAttempts =>
        Set<HotelSupplierReservationAttempt>();

    public DbSet<HotelSupplierReservationIdempotencyRecord> HotelSupplierReservationIdempotency =>
        Set<HotelSupplierReservationIdempotencyRecord>();

    public DbSet<HotelBookingCancellation> HotelBookingCancellations => Set<HotelBookingCancellation>();

    public DbSet<HotelSupplierCancellationAttempt> HotelSupplierCancellationAttempts =>
        Set<HotelSupplierCancellationAttempt>();

    public DbSet<HotelBookingCancellationIdempotencyRecord> HotelBookingCancellationIdempotency =>
        Set<HotelBookingCancellationIdempotencyRecord>();

    public DbSet<HotelBookingReconciliationIssue> HotelBookingReconciliationIssues =>
        Set<HotelBookingReconciliationIssue>();

    public DbSet<HotelBookingPaymentEvidence> HotelBookingPaymentEvidence =>
        Set<HotelBookingPaymentEvidence>();

    public DbSet<HotelBookingPaymentCompensationEvidence> PaymentCompensationEvidence =>
        Set<HotelBookingPaymentCompensationEvidence>();

    public DbSet<HotelBookingRefundInvariantIssue> RefundInvariantIssues =>
        Set<HotelBookingRefundInvariantIssue>();

    public DbSet<HotelBookingOutboxMessage> OutboxMessages => Set<HotelBookingOutboxMessage>();

    public DbSet<HotelBookingPaymentSuccessInboxRecord> PaymentSuccessInbox =>
        Set<HotelBookingPaymentSuccessInboxRecord>();

    public DbSet<HotelBookingRefundSuccessInboxRecord> RefundSuccessInbox =>
        Set<HotelBookingRefundSuccessInboxRecord>();

    public DbSet<HotelBookingAccessCredential> AccessCredentials => Set<HotelBookingAccessCredential>();

    public DbSet<HotelBookingPublicIdempotencyRecord> PublicIdempotency =>
        Set<HotelBookingPublicIdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelBookingDbContext).Assembly);
    }
}

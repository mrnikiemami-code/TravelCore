namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// P19-R3 capacity-consumption invariants. Tour remains capacity-definition owner.
/// </summary>
public static class CapacityConsumptionBoundary
{
    public const string CapacityDefinitionOwner = "Tour";
    public const string CapacityConsumptionOwner = "Booking";
    public const string CapacityDefinitionIsNotCapacityConsumption = "CapacityDefinition != CapacityConsumption";
    public const string CapacityHoldStatusIsNotBookingStatus = "CapacityHoldStatus != BookingStatus";
    public const string PendingIsNotCapacityHeld = "Pending != CapacityHeld";
    public const string ConsumedIsNotBookingConfirmed = "Consumed != BookingConfirmed";
    public const string ExpiredHoldIsNotExpiredBooking = "Expired Hold != Expired Booking";
    public const string ExpiredHoldIsNotBookingExpired = "Expired Hold != BookingExpired";
    public const string HeldSeatCountIsNotBookingPassenger = "HeldSeatCount != BookingPassenger";
    public const string ObservedCapacityIsNotTourSourceOfTruth = "NOT Tour Source of Truth";
    public const string ConcurrencyMechanism = "PostgreSqlAdvisoryTransactionLock";
    public const bool ProcessLocalLockIsAuthoritative = false;
    public const bool ClientInventedConfiguredCapacityIsAuthoritative = false;
    public const bool UnrestrictedBookingConfirmationImplemented = false;
    public const bool PublicHoldSurfaceImplemented = false;
    public const bool HoldDurationHardcoded = false;
}

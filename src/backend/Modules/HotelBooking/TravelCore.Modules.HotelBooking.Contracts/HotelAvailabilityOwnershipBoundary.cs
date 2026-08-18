namespace TravelCore.Modules.HotelBooking.Contracts;

/// <summary>
/// P21-R3: HotelBooking orchestrates a temporary multi-room hold. It is not inventory authority.
/// </summary>
public static class HotelAvailabilityOwnershipBoundary
{
    public const string AvailabilityAuthority = "HotelAvailabilitySource";
    public const string NamedHotelSupplier = "NONE";
    public const string ProductionAvailabilitySource = "NONE";
    public const string HotelBookingIsNotInventoryAuthority = "HotelBooking != Hotel inventory authority";
    public const string PlaceIsNotLiveAvailability = "Place != live availability authority";
    public const string SearchIsNotLiveAvailability = "Search != live availability authority";
    public const string HoldIsNotConfirmation = "HotelAvailabilityHold != HotelBooking confirmation";
    public const string HoldIsNotPayment = "HotelAvailabilityHold != Payment";
    public const string AvailabilityIsNotPrice = "Availability != Price";
    public const string HoldStatuses = "Requested, Active, Released, Expired";
    public const string SourcePortName = "IHotelAvailabilitySource";

    public const bool ProductionFakeSourceImplemented = false;
    public const bool NamedSupplierSdkImplemented = false;
    public const bool AutomaticFailoverImplemented = false;
    public const bool SmartRoutingImplemented = false;
    public const bool HardcodedTtlImplemented = false;
    public const bool ProcessLocalLockIsAuthority = false;
    public const bool HotelBookingStatusImplemented = true;
    public const bool PublicAvailabilityApiImplemented = true;
}

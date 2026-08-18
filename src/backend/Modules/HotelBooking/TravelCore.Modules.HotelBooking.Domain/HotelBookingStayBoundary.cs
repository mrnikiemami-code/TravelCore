namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// P21-R2 stay/room/guest invariants. Lifecycle/status remains R5.
/// </summary>
public static class HotelBookingStayBoundary
{
    public const string CheckInType = "NodaTime.LocalDate";
    public const string CheckOutType = "NodaTime.LocalDate";
    public const string NightsRule = "Nights = CheckOutDate - CheckInDate";
    public const string MultiRoom = "one HotelBooking supports one or more RoomReservations";
    public const string GuestCategories = "Adult, Child";
    public const string ChildAgeRule = "Child requires AgeAtCheckIn";
    public const string BirthDateStored = "NO";
    public const string IdentityConvention = "UUIDv7";

    public const bool HotelBookingStatusImplemented = false;
    public const bool BirthDateStoredFlag = false;
    public const bool PassportStored = false;
    public const bool AvailabilityHoldImplemented = false;
    public const bool SupplierReservationImplemented = false;
    public const bool RateQuoteImplemented = false;
    public const bool CancellationImplemented = false;
    public const bool PaymentIntegrationImplemented = false;
    public const bool MultiRoomSupported = true;
}

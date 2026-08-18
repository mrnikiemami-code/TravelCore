namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// Construction input for one booked room position and its assigned guests.
/// </summary>
public sealed class RoomReservationSpecification
{
    public RoomReservationSpecification(IReadOnlyList<HotelBookingGuestSpecification> guests)
    {
        ArgumentNullException.ThrowIfNull(guests);
        Guests = guests;
    }

    public IReadOnlyList<HotelBookingGuestSpecification> Guests { get; }
}

public sealed class HotelBookingGuestSpecification
{
    public HotelBookingGuestSpecification(
        string givenName,
        string familyName,
        HotelGuestCategory category,
        bool isLeadGuest,
        int? ageAtCheckInYears = null)
    {
        GivenName = givenName;
        FamilyName = familyName;
        Category = category;
        IsLeadGuest = isLeadGuest;
        AgeAtCheckInYears = ageAtCheckInYears;
    }

    public string GivenName { get; }

    public string FamilyName { get; }

    public HotelGuestCategory Category { get; }

    public bool IsLeadGuest { get; }

    public int? AgeAtCheckInYears { get; }
}

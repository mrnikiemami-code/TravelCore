namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// Transaction-time guest snapshot assigned to exactly one RoomReservation.
/// Not Party, Identity, Tour passenger, or TripPlanner traveler.
/// </summary>
public sealed class HotelBookingGuest
{
    public const int NameMaxLength = 100;

    private HotelBookingGuest()
    {
    }

    private HotelBookingGuest(
        HotelBookingGuestId id,
        HotelBookingId hotelBookingId,
        RoomReservationId roomReservationId,
        string givenName,
        string familyName,
        HotelGuestCategory category,
        HotelGuestAgeAtCheckIn? ageAtCheckIn,
        bool isLeadGuest)
    {
        Id = id;
        HotelBookingId = hotelBookingId;
        RoomReservationId = roomReservationId;
        GivenName = givenName;
        FamilyName = familyName;
        Category = category;
        AgeAtCheckIn = ageAtCheckIn;
        IsLeadGuest = isLeadGuest;
    }

    public HotelBookingGuestId Id { get; private set; }

    public HotelBookingId HotelBookingId { get; private set; }

    public RoomReservationId RoomReservationId { get; private set; }

    public string GivenName { get; private set; } = string.Empty;

    public string FamilyName { get; private set; } = string.Empty;

    public HotelGuestCategory Category { get; private set; }

    public HotelGuestAgeAtCheckIn? AgeAtCheckIn { get; private set; }

    public bool IsLeadGuest { get; private set; }

    internal static HotelBookingGuest Create(
        HotelBookingId hotelBookingId,
        RoomReservationId roomReservationId,
        HotelBookingGuestSpecification specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        if (!Enum.IsDefined(specification.Category))
        {
            throw new ArgumentOutOfRangeException(
                nameof(specification),
                specification.Category,
                "HotelGuestCategory is not controlled.");
        }

        HotelGuestAgeAtCheckIn? age = null;
        if (specification.Category == HotelGuestCategory.Child)
        {
            if (specification.AgeAtCheckInYears is null)
            {
                throw new ArgumentException("Child guest requires AgeAtCheckIn.", nameof(specification));
            }

            age = new HotelGuestAgeAtCheckIn(specification.AgeAtCheckInYears.Value);
        }
        else if (specification.AgeAtCheckInYears is not null)
        {
            throw new ArgumentException("Adult guest must not supply AgeAtCheckIn.", nameof(specification));
        }

        return new HotelBookingGuest(
            HotelBookingGuestId.New(),
            hotelBookingId,
            roomReservationId,
            RequireName(specification.GivenName, nameof(specification.GivenName)),
            RequireName(specification.FamilyName, nameof(specification.FamilyName)),
            specification.Category,
            age,
            specification.IsLeadGuest);
    }

    private static string RequireName(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Guest name is required.", paramName);
        }

        var trimmed = value.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"Name max length is {NameMaxLength}.", paramName);
        }

        return trimmed;
    }
}

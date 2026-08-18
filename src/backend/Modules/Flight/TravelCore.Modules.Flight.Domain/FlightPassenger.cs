namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Transaction-time passenger snapshot for one FlightBooking. Not HotelBookingGuest, BookingPassenger,
/// Party person, passport, or fare/seat assignment.
/// </summary>
public sealed class FlightPassenger
{
    public const int NameMaxLength = 100;

    private FlightPassenger()
    {
    }

    private FlightPassenger(
        FlightPassengerId id,
        FlightBookingId flightBookingId,
        int ordinal,
        string givenName,
        string familyName,
        FlightPassengerCategory category)
    {
        Id = id;
        FlightBookingId = flightBookingId;
        Ordinal = ordinal;
        GivenName = givenName;
        FamilyName = familyName;
        Category = category;
    }

    public FlightPassengerId Id { get; private set; }

    public FlightBookingId FlightBookingId { get; private set; }

    public int Ordinal { get; private set; }

    public string GivenName { get; private set; } = string.Empty;

    public string FamilyName { get; private set; } = string.Empty;

    public FlightPassengerCategory Category { get; private set; }

    internal static FlightPassenger Create(
        FlightBookingId flightBookingId,
        int ordinal,
        FlightPassengerSpecification specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        if (ordinal < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, "Passenger ordinal must be >= 1.");
        }

        if (!Enum.IsDefined(specification.Category))
        {
            throw new ArgumentOutOfRangeException(
                nameof(specification),
                specification.Category,
                "FlightPassengerCategory is not controlled.");
        }

        return new FlightPassenger(
            FlightPassengerId.New(),
            flightBookingId,
            ordinal,
            RequireName(specification.GivenName, nameof(specification.GivenName)),
            RequireName(specification.FamilyName, nameof(specification.FamilyName)),
            specification.Category);
    }

    private static string RequireName(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Passenger name is required.", paramName);
        }

        var trimmed = value.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"Name max length is {NameMaxLength}.", paramName);
        }

        return trimmed;
    }
}

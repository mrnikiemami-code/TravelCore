using NodaTime;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// One marketed/operated flight between two airports inside a FlightJourney.
/// Not a FlightLeg, aircraft, seat, cabin, fare, or PNR.
/// </summary>
public sealed class FlightSegment
{
    public const int FlightNumberMaxLength = 8;

    private FlightSegment()
    {
        Origin = default;
        Destination = default;
        DepartureTimeZoneId = null!;
        ArrivalTimeZoneId = null!;
        MarketingCarrier = default;
    }

    private FlightSegment(
        FlightSegmentId id,
        FlightJourneyId flightJourneyId,
        FlightBookingId flightBookingId,
        int ordinal,
        AirportReference origin,
        AirportReference destination,
        Instant departureAt,
        string departureTimeZoneId,
        Instant arrivalAt,
        string arrivalTimeZoneId,
        AirlineReference marketingCarrier,
        AirlineReference? operatingCarrier,
        string? flightNumber)
    {
        Id = id;
        FlightJourneyId = flightJourneyId;
        FlightBookingId = flightBookingId;
        Ordinal = ordinal;
        Origin = origin;
        Destination = destination;
        DepartureAt = departureAt;
        DepartureTimeZoneId = departureTimeZoneId;
        ArrivalAt = arrivalAt;
        ArrivalTimeZoneId = arrivalTimeZoneId;
        MarketingCarrier = marketingCarrier;
        OperatingCarrier = operatingCarrier;
        FlightNumber = flightNumber;
    }

    public FlightSegmentId Id { get; private set; }

    public FlightJourneyId FlightJourneyId { get; private set; }

    public FlightBookingId FlightBookingId { get; private set; }

    public int Ordinal { get; private set; }

    public AirportReference Origin { get; private set; }

    public AirportReference Destination { get; private set; }

    public Instant DepartureAt { get; private set; }

    public string DepartureTimeZoneId { get; private set; }

    public Instant ArrivalAt { get; private set; }

    public string ArrivalTimeZoneId { get; private set; }

    public AirlineReference MarketingCarrier { get; private set; }

    public AirlineReference? OperatingCarrier { get; private set; }

    public string? FlightNumber { get; private set; }

    internal static FlightSegment Create(
        FlightBookingId flightBookingId,
        FlightJourneyId flightJourneyId,
        int ordinal,
        FlightSegmentSpecification specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        if (ordinal < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, "Segment ordinal must be >= 1.");
        }

        if (specification.Origin.IataCode == specification.Destination.IataCode)
        {
            throw new ArgumentException("Segment origin and destination airports must differ.", nameof(specification));
        }

        if (specification.ArrivalAt <= specification.DepartureAt)
        {
            throw new ArgumentException("ArrivalAt must be later than DepartureAt.", nameof(specification));
        }

        return new FlightSegment(
            FlightSegmentId.New(),
            flightJourneyId,
            flightBookingId,
            ordinal,
            specification.Origin,
            specification.Destination,
            specification.DepartureAt,
            FlightTimeZone.RequireIanaId(specification.DepartureTimeZoneId, nameof(specification.DepartureTimeZoneId)),
            specification.ArrivalAt,
            FlightTimeZone.RequireIanaId(specification.ArrivalTimeZoneId, nameof(specification.ArrivalTimeZoneId)),
            specification.MarketingCarrier,
            specification.OperatingCarrier,
            NormalizeFlightNumber(specification.FlightNumber));
    }

    private static string? NormalizeFlightNumber(string? flightNumber)
    {
        if (string.IsNullOrWhiteSpace(flightNumber))
        {
            return null;
        }

        var normalized = flightNumber.Trim().ToUpperInvariant();
        if (normalized.Length > FlightNumberMaxLength
            || !normalized.All(static c => char.IsAsciiLetterOrDigit(c)))
        {
            throw new ArgumentException(
                $"Flight number must be 1..{FlightNumberMaxLength} ASCII letters or digits.",
                nameof(flightNumber));
        }

        return normalized;
    }
}

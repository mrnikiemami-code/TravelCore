namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// One directional journey inside a FlightBooking. Connecting flights are multiple segments.
/// </summary>
public sealed class FlightJourney
{
    private readonly List<FlightSegment> _segments = [];

    private FlightJourney()
    {
    }

    private FlightJourney(FlightJourneyId id, FlightBookingId flightBookingId, int ordinal)
    {
        Id = id;
        FlightBookingId = flightBookingId;
        Ordinal = ordinal;
    }

    public FlightJourneyId Id { get; private set; }

    public FlightBookingId FlightBookingId { get; private set; }

    public int Ordinal { get; private set; }

    public IReadOnlyList<FlightSegment> Segments => _segments;

    public int SegmentCount => _segments.Count;

    public AirportReference Origin => _segments.OrderBy(s => s.Ordinal).First().Origin;

    public AirportReference Destination => _segments.OrderBy(s => s.Ordinal).Last().Destination;

    internal static FlightJourney Create(
        FlightBookingId flightBookingId,
        int ordinal,
        FlightJourneySpecification specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(specification.Segments);
        if (ordinal < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, "Journey ordinal must be >= 1.");
        }

        if (specification.Segments.Count == 0)
        {
            throw new ArgumentException("FlightJourney requires at least one FlightSegment.", nameof(specification));
        }

        var journey = new FlightJourney(FlightJourneyId.New(), flightBookingId, ordinal);
        var segmentOrdinal = 1;
        foreach (var spec in specification.Segments)
        {
            ArgumentNullException.ThrowIfNull(spec);
            journey._segments.Add(FlightSegment.Create(flightBookingId, journey.Id, segmentOrdinal, spec));
            segmentOrdinal++;
        }

        for (var i = 1; i < journey._segments.Count; i++)
        {
            var previous = journey._segments[i - 1];
            var next = journey._segments[i];
            if (previous.Destination.IataCode != next.Origin.IataCode)
            {
                throw new ArgumentException(
                    "Connecting segments require previous destination to equal next origin.",
                    nameof(specification));
            }

            if (next.DepartureAt < previous.ArrivalAt)
            {
                throw new ArgumentException(
                    "Connecting segment departure must be at or after previous arrival.",
                    nameof(specification));
            }
        }

        return journey;
    }
}

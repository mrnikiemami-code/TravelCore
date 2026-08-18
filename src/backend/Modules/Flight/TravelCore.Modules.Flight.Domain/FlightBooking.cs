namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Flight-owned live-flight transaction aggregate (P22-R2). No lifecycle status, fare, PNR, or Payment.
/// </summary>
public sealed class FlightBooking
{
    private readonly List<FlightJourney> _journeys = [];
    private readonly List<FlightPassenger> _passengers = [];

    private FlightBooking()
    {
    }

    private FlightBooking(FlightBookingId id, FlightTripType tripType)
    {
        Id = id;
        TripType = tripType;
    }

    public FlightBookingId Id { get; private set; }

    public FlightTripType TripType { get; private set; }

    public IReadOnlyList<FlightJourney> Journeys => _journeys;

    public IReadOnlyList<FlightPassenger> Passengers => _passengers;

    public int JourneyCount => _journeys.Count;

    public int PassengerCount => _passengers.Count;

    public FlightJourney Outbound => _journeys.OrderBy(j => j.Ordinal).First();

    public FlightJourney? ReturnJourney =>
        TripType == FlightTripType.RoundTrip ? _journeys.OrderBy(j => j.Ordinal).Skip(1).First() : null;

    public static FlightBooking Create(
        FlightTripType tripType,
        IReadOnlyList<FlightJourneySpecification> journeys,
        IReadOnlyList<FlightPassengerSpecification> passengers)
    {
        if (!Enum.IsDefined(tripType))
        {
            throw new ArgumentOutOfRangeException(nameof(tripType), tripType, "FlightTripType is not controlled.");
        }

        ArgumentNullException.ThrowIfNull(journeys);
        ArgumentNullException.ThrowIfNull(passengers);

        var expectedJourneys = tripType switch
        {
            FlightTripType.OneWay => 1,
            FlightTripType.RoundTrip => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(tripType), tripType, "FlightTripType is not controlled."),
        };

        if (journeys.Count != expectedJourneys)
        {
            throw new ArgumentException(
                $"{tripType} FlightBooking requires exactly {expectedJourneys} journey(s).",
                nameof(journeys));
        }

        if (passengers.Count == 0)
        {
            throw new ArgumentException("FlightBooking requires at least one passenger.", nameof(passengers));
        }

        var booking = new FlightBooking(FlightBookingId.New(), tripType);
        var journeyOrdinal = 1;
        foreach (var spec in journeys)
        {
            ArgumentNullException.ThrowIfNull(spec);
            booking._journeys.Add(FlightJourney.Create(booking.Id, journeyOrdinal, spec));
            journeyOrdinal++;
        }

        if (tripType == FlightTripType.RoundTrip)
        {
            var outbound = booking._journeys[0];
            var inbound = booking._journeys[1];
            if (inbound.Origin.IataCode != outbound.Destination.IataCode
                || inbound.Destination.IataCode != outbound.Origin.IataCode)
            {
                throw new ArgumentException(
                    "RoundTrip return origin/destination must reverse the outbound journey.",
                    nameof(journeys));
            }
        }

        var passengerOrdinal = 1;
        foreach (var spec in passengers)
        {
            ArgumentNullException.ThrowIfNull(spec);
            booking._passengers.Add(FlightPassenger.Create(booking.Id, passengerOrdinal, spec));
            passengerOrdinal++;
        }

        if (!booking._passengers.Any(p => p.Category == FlightPassengerCategory.Adult))
        {
            throw new ArgumentException("FlightBooking requires at least one Adult passenger.", nameof(passengers));
        }

        return booking;
    }
}

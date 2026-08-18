using NodaTime;

namespace TravelCore.Modules.Flight.Contracts;

public enum FlightSearchCompletion : short
{
    Complete = 1,
    Unknown = 2,
}

public sealed class FlightPassengerCount
{
    public FlightPassengerCount(int adultCount, int childCount = 0, int infantCount = 0)
    {
        if (adultCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(adultCount), adultCount, "Search requires at least one Adult.");
        }

        if (childCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(childCount), childCount, "Child count cannot be negative.");
        }

        if (infantCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(infantCount), infantCount, "Infant count cannot be negative.");
        }

        AdultCount = adultCount;
        ChildCount = childCount;
        InfantCount = infantCount;
    }

    public int AdultCount { get; }

    public int ChildCount { get; }

    public int InfantCount { get; }

    public int TotalCount => AdultCount + ChildCount + InfantCount;
}

public sealed class FlightSearchRequest
{
    public FlightSearchRequest(
        AirportReference origin,
        AirportReference destination,
        FlightTripType tripType,
        LocalDate departureDate,
        FlightPassengerCount passengers,
        LocalDate? returnDate = null,
        bool directOnly = false)
    {
        if (!Enum.IsDefined(tripType))
        {
            throw new ArgumentOutOfRangeException(nameof(tripType), tripType, "FlightTripType is not controlled. MultiCity is DEFERRED.");
        }

        ArgumentNullException.ThrowIfNull(passengers);

        if (origin.IataCode == destination.IataCode)
        {
            throw new ArgumentException("Origin and destination airports must differ.", nameof(destination));
        }

        if (tripType == FlightTripType.OneWay)
        {
            if (returnDate is not null)
            {
                throw new ArgumentException("OneWay search must not include a return date.", nameof(returnDate));
            }
        }
        else if (tripType == FlightTripType.RoundTrip)
        {
            if (returnDate is null)
            {
                throw new ArgumentException("RoundTrip search requires a return date.", nameof(returnDate));
            }

            if (returnDate.Value < departureDate)
            {
                throw new ArgumentException("RoundTrip return date cannot be before departure date.", nameof(returnDate));
            }
        }

        Origin = origin;
        Destination = destination;
        TripType = tripType;
        DepartureDate = departureDate;
        ReturnDate = returnDate;
        Passengers = passengers;
        DirectOnly = directOnly;
    }

    public AirportReference Origin { get; }

    public AirportReference Destination { get; }

    public FlightTripType TripType { get; }

    public LocalDate DepartureDate { get; }

    public LocalDate? ReturnDate { get; }

    public FlightPassengerCount Passengers { get; }

    public bool DirectOnly { get; }
}

public sealed class FlightSearchSegment
{
    public const int FlightNumberMaxLength = 8;

    public FlightSearchSegment(
        int ordinal,
        AirportReference origin,
        AirportReference destination,
        AirlineReference marketingCarrier,
        Instant departureAt,
        string departureTimeZoneId,
        Instant arrivalAt,
        string arrivalTimeZoneId,
        AirlineReference? operatingCarrier = null,
        string? flightNumber = null)
    {
        if (ordinal < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, "Segment ordinal must be >= 1.");
        }

        if (origin.IataCode == destination.IataCode)
        {
            throw new ArgumentException("Segment origin and destination must differ.", nameof(destination));
        }

        if (arrivalAt <= departureAt)
        {
            throw new ArgumentException("ArrivalAt must be after DepartureAt.", nameof(arrivalAt));
        }

        Origin = origin;
        Destination = destination;
        MarketingCarrier = marketingCarrier;
        OperatingCarrier = operatingCarrier;
        Ordinal = ordinal;
        DepartureAt = departureAt;
        DepartureTimeZoneId = FlightTimeZone.RequireIanaId(departureTimeZoneId, nameof(departureTimeZoneId));
        ArrivalAt = arrivalAt;
        ArrivalTimeZoneId = FlightTimeZone.RequireIanaId(arrivalTimeZoneId, nameof(arrivalTimeZoneId));
        FlightNumber = NormalizeFlightNumber(flightNumber);
    }

    public int Ordinal { get; }

    public AirportReference Origin { get; }

    public AirportReference Destination { get; }

    public AirlineReference MarketingCarrier { get; }

    public AirlineReference? OperatingCarrier { get; }

    public Instant DepartureAt { get; }

    public string DepartureTimeZoneId { get; }

    public Instant ArrivalAt { get; }

    public string ArrivalTimeZoneId { get; }

    public string? FlightNumber { get; }

    private static string? NormalizeFlightNumber(string? flightNumber)
    {
        if (string.IsNullOrWhiteSpace(flightNumber))
        {
            return null;
        }

        var normalized = flightNumber.Trim().ToUpperInvariant();
        if (normalized.Length is < 1 or > FlightNumberMaxLength
            || !normalized.All(static c => char.IsAsciiLetterOrDigit(c)))
        {
            throw new ArgumentException(
                "Flight number must be 1..8 alphanumeric characters.",
                nameof(flightNumber));
        }

        return normalized;
    }
}

public sealed class FlightSearchJourney
{
    public FlightSearchJourney(int ordinal, IReadOnlyList<FlightSearchSegment> segments)
    {
        if (ordinal < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, "Journey ordinal must be >= 1.");
        }

        ArgumentNullException.ThrowIfNull(segments);
        if (segments.Count == 0)
        {
            throw new ArgumentException("A journey requires at least one segment.", nameof(segments));
        }

        var ordered = segments.OrderBy(s => s.Ordinal).ToArray();
        for (var i = 0; i < ordered.Length; i++)
        {
            if (ordered[i].Ordinal != i + 1)
            {
                throw new ArgumentException("Segment ordinals must be consecutive starting at 1.", nameof(segments));
            }
        }

        for (var i = 1; i < ordered.Length; i++)
        {
            var previous = ordered[i - 1];
            var next = ordered[i];
            if (previous.Destination.IataCode != next.Origin.IataCode)
            {
                throw new ArgumentException("Connecting segments must match destination to next origin.", nameof(segments));
            }

            if (next.DepartureAt < previous.ArrivalAt)
            {
                throw new ArgumentException("Next segment cannot depart before the previous arrival.", nameof(segments));
            }
        }

        Ordinal = ordinal;
        Segments = ordered;
    }

    public int Ordinal { get; }

    public IReadOnlyList<FlightSearchSegment> Segments { get; }

    public AirportReference Origin => Segments[0].Origin;

    public AirportReference Destination => Segments[^1].Destination;
}

public sealed class FlightSearchOption
{
    public const int SourceOptionReferenceMaxLength = 128;

    public FlightSearchOption(
        FlightSourceKey sourceKey,
        string sourceOptionReference,
        FlightTripType tripType,
        IReadOnlyList<FlightSearchJourney> journeys,
        Instant observedAt,
        Instant? expiresAt = null)
    {
        if (string.IsNullOrWhiteSpace(sourceOptionReference))
        {
            throw new ArgumentException("SourceOptionReference is required.", nameof(sourceOptionReference));
        }

        var reference = sourceOptionReference.Trim();
        if (reference.Length > SourceOptionReferenceMaxLength)
        {
            throw new ArgumentException(
                $"SourceOptionReference max length is {SourceOptionReferenceMaxLength}.",
                nameof(sourceOptionReference));
        }

        if (!Enum.IsDefined(tripType))
        {
            throw new ArgumentOutOfRangeException(nameof(tripType), tripType, "FlightTripType is not controlled.");
        }

        ArgumentNullException.ThrowIfNull(journeys);
        var expected = tripType == FlightTripType.OneWay ? 1 : 2;
        if (journeys.Count != expected)
        {
            throw new ArgumentException($"{tripType} search option requires exactly {expected} journey(s).", nameof(journeys));
        }

        var ordered = journeys.OrderBy(j => j.Ordinal).ToArray();
        for (var i = 0; i < ordered.Length; i++)
        {
            if (ordered[i].Ordinal != i + 1)
            {
                throw new ArgumentException("Journey ordinals must be consecutive starting at 1.", nameof(journeys));
            }
        }

        if (tripType == FlightTripType.RoundTrip)
        {
            var outbound = ordered[0];
            var inbound = ordered[1];
            if (inbound.Origin.IataCode != outbound.Destination.IataCode
                || inbound.Destination.IataCode != outbound.Origin.IataCode)
            {
                throw new ArgumentException(
                    "RoundTrip return origin/destination must reverse the outbound journey.",
                    nameof(journeys));
            }
        }

        if (expiresAt is { } expiry && expiry <= observedAt)
        {
            throw new ArgumentException("ExpiresAt must be after ObservedAt when supplied.", nameof(expiresAt));
        }

        SourceKey = sourceKey;
        SourceOptionReference = reference;
        TripType = tripType;
        Journeys = ordered;
        ObservedAt = observedAt;
        ExpiresAt = expiresAt;
    }

    public FlightSourceKey SourceKey { get; }

    public string SourceOptionReference { get; }

    public FlightTripType TripType { get; }

    public IReadOnlyList<FlightSearchJourney> Journeys { get; }

    public Instant ObservedAt { get; }

    public Instant? ExpiresAt { get; }
}

public sealed class FlightSearchResult
{
    private FlightSearchResult(
        FlightSearchCompletion completion,
        FlightSourceKey? sourceKey,
        Instant observedAt,
        IReadOnlyList<FlightSearchOption> options)
    {
        Completion = completion;
        SourceKey = sourceKey;
        ObservedAt = observedAt;
        Options = options;
    }

    public FlightSearchCompletion Completion { get; }

    public FlightSourceKey? SourceKey { get; }

    public Instant ObservedAt { get; }

    public IReadOnlyList<FlightSearchOption> Options { get; }

    public static FlightSearchResult Complete(
        FlightSourceKey sourceKey,
        Instant observedAt,
        IReadOnlyList<FlightSearchOption> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        foreach (var option in options)
        {
            if (option.SourceKey.Value != sourceKey.Value)
            {
                throw new ArgumentException("Search options must belong to the same SourceKey.", nameof(options));
            }
        }

        return new FlightSearchResult(FlightSearchCompletion.Complete, sourceKey, observedAt, options);
    }

    public static FlightSearchResult ZeroSource(Instant observedAt) =>
        new(FlightSearchCompletion.Complete, sourceKey: null, observedAt, []);

    public static FlightSearchResult UnknownTimeout(FlightSourceKey sourceKey, Instant observedAt) =>
        new(FlightSearchCompletion.Unknown, sourceKey, observedAt, []);
}

using NodaTime;

namespace TravelCore.Modules.Flight.Domain;

public sealed record FlightSegmentSpecification(
    AirportReference Origin,
    AirportReference Destination,
    Instant DepartureAt,
    string DepartureTimeZoneId,
    Instant ArrivalAt,
    string ArrivalTimeZoneId,
    AirlineReference MarketingCarrier,
    AirlineReference? OperatingCarrier,
    string? FlightNumber);

public sealed record FlightJourneySpecification(IReadOnlyList<FlightSegmentSpecification> Segments);

public sealed record FlightPassengerSpecification(
    string GivenName,
    string FamilyName,
    FlightPassengerCategory Category);

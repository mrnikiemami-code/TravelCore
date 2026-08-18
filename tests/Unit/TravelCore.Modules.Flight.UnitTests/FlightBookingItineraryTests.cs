using NodaTime;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using Xunit;

namespace TravelCore.Modules.Flight.UnitTests;

public sealed class FlightBookingItineraryTests
{
    private static readonly Instant T0 = Instant.FromUtc(2026, 9, 1, 6, 0);
    private static readonly Instant T1 = Instant.FromUtc(2026, 9, 1, 10, 0);
    private static readonly Instant T2 = Instant.FromUtc(2026, 9, 1, 12, 0);
    private static readonly Instant T3 = Instant.FromUtc(2026, 9, 1, 16, 0);
    private static readonly Instant T4 = Instant.FromUtc(2026, 9, 8, 8, 0);
    private static readonly Instant T5 = Instant.FromUtc(2026, 9, 8, 14, 0);

    [Fact]
    public void OneWay_Valid_Connecting_Journey_Is_Accepted()
    {
        var booking = FlightBooking.Create(
            FlightTripType.OneWay,
            [ConnectingThrIstLhr()],
            [Adult("Ada", "Lovelace"), Child("Alan", "Turing"), Infant("Grace", "Hopper")]);

        Assert.Equal(FlightTripType.OneWay, booking.TripType);
        Assert.Equal(1, booking.JourneyCount);
        Assert.Equal(2, booking.Outbound.SegmentCount);
        Assert.Equal("THR", booking.Outbound.Origin.IataCode);
        Assert.Equal("LHR", booking.Outbound.Destination.IataCode);
        Assert.Null(booking.ReturnJourney);
        Assert.Equal(3, booking.PassengerCount);
        Assert.Contains(booking.Passengers, p => p.Category == FlightPassengerCategory.Infant);
        Assert.Equal([1, 2], booking.Outbound.Segments.Select(s => s.Ordinal).ToArray());
    }

    [Fact]
    public void OneWay_With_Two_Journeys_Is_Rejected()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            FlightBooking.Create(
                FlightTripType.OneWay,
                [Direct("THR", "IST", T0, T1), Direct("IST", "THR", T4, T5)],
                [Adult("Ada", "Lovelace")]));
        Assert.Contains("exactly 1", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RoundTrip_Valid_Is_Accepted()
    {
        var booking = FlightBooking.Create(
            FlightTripType.RoundTrip,
            [Direct("THR", "LHR", T0, T1), Direct("LHR", "THR", T4, T5)],
            [Adult("Ada", "Lovelace")]);

        Assert.Equal(2, booking.JourneyCount);
        Assert.NotNull(booking.ReturnJourney);
        Assert.Equal("LHR", booking.ReturnJourney!.Origin.IataCode);
        Assert.Equal("THR", booking.ReturnJourney.Destination.IataCode);
    }

    [Fact]
    public void RoundTrip_With_One_Journey_Is_Rejected()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            FlightBooking.Create(
                FlightTripType.RoundTrip,
                [Direct("THR", "LHR", T0, T1)],
                [Adult("Ada", "Lovelace")]));
        Assert.Contains("exactly 2", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Disconnected_Connecting_Airports_Are_Rejected()
    {
        var broken = new FlightJourneySpecification(
        [
            Segment("THR", "IST", T0, T1),
            Segment("AYT", "LHR", T2, T3),
        ]);

        var ex = Assert.Throws<ArgumentException>(() =>
            FlightBooking.Create(FlightTripType.OneWay, [broken], [Adult("Ada", "Lovelace")]));
        Assert.Contains("previous destination", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Chronological_Segment_Violation_Is_Rejected()
    {
        var broken = new FlightJourneySpecification(
        [
            Segment("THR", "IST", T2, T3),
            Segment("IST", "LHR", T0, T1),
        ]);

        var ex = Assert.Throws<ArgumentException>(() =>
            FlightBooking.Create(FlightTripType.OneWay, [broken], [Adult("Ada", "Lovelace")]));
        Assert.Contains("at or after previous arrival", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Arrival_Not_After_Departure_Is_Rejected()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            FlightBooking.Create(
                FlightTripType.OneWay,
                [Direct("THR", "IST", T1, T0)],
                [Adult("Ada", "Lovelace")]));
        Assert.Contains("ArrivalAt must be later", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RoundTrip_Reverse_Endpoint_Mismatch_Is_Rejected()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            FlightBooking.Create(
                FlightTripType.RoundTrip,
                [Direct("THR", "LHR", T0, T1), Direct("IST", "THR", T4, T5)],
                [Adult("Ada", "Lovelace")]));
        Assert.Contains("reverse the outbound", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void No_Passengers_Is_Rejected()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            FlightBooking.Create(FlightTripType.OneWay, [Direct("THR", "IST", T0, T1)], []));
        Assert.Contains("at least one passenger", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void No_Adult_Is_Rejected()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            FlightBooking.Create(
                FlightTripType.OneWay,
                [Direct("THR", "IST", T0, T1)],
                [Child("Alan", "Turing"), Infant("Grace", "Hopper")]));
        Assert.Contains("at least one Adult", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Airport_Iata_Is_Normalized_And_Rejected_When_Invalid()
    {
        Assert.Equal("THR", new AirportReference("thr").IataCode);
        Assert.Throws<ArgumentException>(() => new AirportReference("TH"));
        Assert.Throws<ArgumentException>(() => new AirportReference("THRX"));
    }

    private static FlightJourneySpecification ConnectingThrIstLhr() =>
        new(
        [
            Segment("THR", "IST", T0, T1),
            Segment("IST", "LHR", T2, T3),
        ]);

    private static FlightJourneySpecification Direct(
        string origin,
        string destination,
        Instant departure,
        Instant arrival) =>
        new([Segment(origin, destination, departure, arrival)]);

    private static FlightSegmentSpecification Segment(
        string origin,
        string destination,
        Instant departure,
        Instant arrival) =>
        new(
            new AirportReference(origin),
            new AirportReference(destination),
            departure,
            "Asia/Tehran",
            arrival,
            "Europe/London",
            new AirlineReference("TK"),
            OperatingCarrier: null,
            FlightNumber: "TK123");

    private static FlightPassengerSpecification Adult(string given, string family) =>
        new(given, family, FlightPassengerCategory.Adult);

    private static FlightPassengerSpecification Child(string given, string family) =>
        new(given, family, FlightPassengerCategory.Child);

    private static FlightPassengerSpecification Infant(string given, string family) =>
        new(given, family, FlightPassengerCategory.Infant);
}

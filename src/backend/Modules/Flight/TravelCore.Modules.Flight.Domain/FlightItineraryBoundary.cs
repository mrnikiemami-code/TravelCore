namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// P22-R2 itinerary / reference / passenger invariants. T005 owns supplier reservation / PNR correlation;
/// ticketing, Payment, and FlightBookingStatus remain later.
/// </summary>
public static class FlightItineraryBoundary
{
    public const string TripTypes = "OneWay, RoundTrip";
    public const string MultiCity = "DEFERRED";
    public const string OneWayJourneyCount = "1";
    public const string RoundTripJourneyCount = "2";
    public const string Structure = "FlightBooking -> FlightJourney -> FlightSegment";
    public const string ConnectingSegments = "YES";
    public const string FlightLegPresent = "NO";
    public const string AirportAuthority = "ReferenceData";
    public const string AirlineAuthority = "ReferenceData";
    public const string AirportReferenceRepresentation = "AirportReference(IATA 3-letter code)";
    public const string AirlineReferenceRepresentation = "AirlineReference(IATA 2-character code)";
    public const string DepartureType = "NodaTime.Instant";
    public const string ArrivalType = "NodaTime.Instant";
    public const string TimeZoneRepresentation = "IANA timezone identifier";
    public const string PassengerCategories = "Adult, Child, Infant";
    public const string MinimumAdultRule = "at least one Adult";
    public const string BirthDateStored = "NO";
    public const string GenderStored = "NO";
    public const string NationalityStored = "NO";
    public const string PassportStored = "NO";

    public const bool FlightBookingStatusImplemented = false;
    public const bool FlightLegImplemented = false;
    public const bool MultiCityImplemented = false;
    public const bool ConnectingSegmentsSupported = true;
    public const bool BirthDateStoredFlag = false;
    public const bool SearchImplemented = true;
    public const bool OfferImplemented = true;
    public const bool ReservationImplemented = true;
    public const bool PnrImplemented = false;
    public const bool TicketImplemented = false;
    public const bool PaymentIntegrationImplemented = false;
}

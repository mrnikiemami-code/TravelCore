namespace TravelCore.Modules.Flight.Contracts;

public sealed record PublicFlightSearchRequest(
    string OriginIata,
    string DestinationIata,
    string TripType,
    DateOnly DepartureDate,
    DateOnly? ReturnDate,
    int AdultCount,
    int ChildCount,
    int InfantCount);

public sealed record PublicFlightSearchSegmentRead(
    int Ordinal,
    string OriginIata,
    string DestinationIata,
    DateTimeOffset DepartureAt,
    string DepartureTimeZoneId,
    DateTimeOffset ArrivalAt,
    string ArrivalTimeZoneId,
    string MarketingCarrierIata,
    string? OperatingCarrierIata,
    string? FlightNumber);

public sealed record PublicFlightSearchJourneyRead(
    int Ordinal,
    IReadOnlyList<PublicFlightSearchSegmentRead> Segments);

public sealed record PublicFlightSearchOptionRead(
    string SourceOptionReference,
    string TripType,
    IReadOnlyList<PublicFlightSearchJourneyRead> Journeys,
    DateTimeOffset ObservedAt,
    DateTimeOffset? ExpiresAt);

public sealed record PublicFlightSearchResultRead(
    string Completion,
    bool SourceConfigured,
    string? SafeMessage,
    IReadOnlyList<PublicFlightSearchOptionRead> Options);

public sealed record PublicFlightSegmentInput(
    string OriginIata,
    string DestinationIata,
    DateTimeOffset DepartureAt,
    string DepartureTimeZoneId,
    DateTimeOffset ArrivalAt,
    string ArrivalTimeZoneId,
    string MarketingCarrierIata,
    string? OperatingCarrierIata,
    string? FlightNumber);

public sealed record PublicFlightJourneyInput(IReadOnlyList<PublicFlightSegmentInput> Segments);

public sealed record PublicFlightPassengerInput(string GivenName, string FamilyName, string Category);

public sealed record PublicFlightBookingInitiationRequest(
    string TripType,
    IReadOnlyList<PublicFlightJourneyInput> Journeys,
    IReadOnlyList<PublicFlightPassengerInput> Passengers,
    string? IdempotencyKey);

public sealed record PublicFlightPassengerRead(
    Guid PassengerId,
    string GivenName,
    string FamilyName,
    string Category);

public sealed record PublicFlightSegmentRead(
    Guid SegmentId,
    int Ordinal,
    string OriginIata,
    string DestinationIata,
    DateTimeOffset DepartureAt,
    string DepartureTimeZoneId,
    DateTimeOffset ArrivalAt,
    string ArrivalTimeZoneId,
    string MarketingCarrierIata,
    string? OperatingCarrierIata,
    string? FlightNumber);

public sealed record PublicFlightJourneyRead(
    Guid JourneyId,
    int Ordinal,
    IReadOnlyList<PublicFlightSegmentRead> Segments);

public sealed record PublicFlightBaggageRead(
    int? Quantity,
    decimal? Weight,
    string? Unit,
    string? Category,
    string? PassengerCategory);

public sealed record PublicFlightFareRulesRead(
    bool Refundable,
    bool Changeable,
    DateTimeOffset? TicketingDeadline,
    decimal? CancelPenaltyAmount,
    string? CancelPenaltyCurrencyCode,
    IReadOnlyList<PublicFlightBaggageRead> Baggage);

public sealed record PublicFlightOfferRead(
    Guid SnapshotId,
    string CurrencyCode,
    decimal TotalAmount,
    DateTimeOffset? OfferExpiresAt,
    bool OfferExpired,
    DateTimeOffset? TicketingDeadline,
    PublicFlightFareRulesRead? FareRules);

public sealed record PublicFlightReservationRead(
    string PresentationStatus,
    string? ReservationLocator,
    DateTimeOffset? ExpiresAt);

public sealed record PublicFlightTicketRead(
    Guid PassengerId,
    string Status,
    string? TicketNumber);

public sealed record PublicFlightCancellationRead(
    string Status,
    string? FinancialOutcome,
    decimal? PenaltyAmount,
    decimal? RefundAmount,
    string? CurrencyCode);

public sealed record PublicFlightBookingInitiationResponse(
    Guid FlightBookingId,
    string Status,
    string PresentationState,
    string? AccessToken,
    bool AccessTokenIssued,
    bool Confirmed,
    string TripType,
    IReadOnlyList<PublicFlightJourneyRead> Journeys,
    IReadOnlyList<PublicFlightPassengerRead> Passengers);

public sealed record PublicFlightBookingRead(
    Guid FlightBookingId,
    string Status,
    string PresentationState,
    bool Confirmed,
    string TripType,
    IReadOnlyList<PublicFlightJourneyRead> Journeys,
    IReadOnlyList<PublicFlightPassengerRead> Passengers,
    PublicFlightOfferRead? Offer,
    PublicFlightReservationRead? Reservation,
    IReadOnlyList<PublicFlightTicketRead> Tickets,
    PublicFlightCancellationRead? Cancellation,
    string? PaymentStatus,
    string? RefundStatus,
    bool CancellationAvailable,
    bool OfferExpired,
    string? SafeMessage);

public sealed record PublicFlightBookingPaymentRead(
    Guid FlightBookingId,
    string FlightBookingStatus,
    bool FlightBookingConfirmed,
    string PresentationState,
    Guid? PaymentId,
    string? PaymentStatus,
    decimal? Amount,
    string? CurrencyCode,
    bool ProviderInitiationPossible,
    string? LatestAttemptStatus,
    string? RefundStatus,
    string SafeAction,
    string? RedirectUri,
    PublicFlightOfferRead? Offer);

public sealed record PublicFlightBookingCancellationCommandResult(
    string Outcome,
    PublicFlightBookingRead Booking);

public interface IPublicFlightBookingSearchService
{
    Task<PublicFlightSearchResultRead> SearchAsync(
        PublicFlightSearchRequest request,
        CancellationToken cancellationToken = default);
}

public interface IPublicFlightBookingInitiationService
{
    Task<PublicFlightBookingInitiationResponse> InitiateAsync(
        PublicFlightBookingInitiationRequest request,
        Guid? actorId,
        CancellationToken cancellationToken = default);
}

public interface IPublicFlightBookingReadService
{
    Task<PublicFlightBookingRead?> GetAuthorizedAsync(
        Guid flightBookingId,
        string? accessToken,
        Guid? actorId,
        CancellationToken cancellationToken = default);
}

public interface IPublicFlightBookingJourneyService
{
    Task<PublicFlightBookingProgressResult> AcceptOfferAsync(
        Guid flightBookingId,
        string? accessToken,
        Guid? actorId,
        string? idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<PublicFlightBookingProgressResult> RequestReservationAsync(
        Guid flightBookingId,
        string? accessToken,
        Guid? actorId,
        string? idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<PublicFlightBookingCancellationCommandResult?> RequestCancellationAsync(
        Guid flightBookingId,
        string? accessToken,
        Guid? actorId,
        string? idempotencyKey,
        CancellationToken cancellationToken = default);
}

public sealed record PublicFlightBookingProgressResult(
    PublicFlightBookingJourneyStatus Status,
    PublicFlightBookingRead Booking);

public enum PublicFlightBookingJourneyStatus
{
    Completed = 0,
    SourceUnavailable = 1,
    Ineligible = 2,
    OfferExpired = 3,
    OfferRequoteRequired = 4,
    OfferUnavailable = 5,
}

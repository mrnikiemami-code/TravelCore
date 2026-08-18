namespace TravelCore.Modules.HotelBooking.Contracts;

public sealed record PublicHotelBookingGuestInput(
    string GivenName,
    string FamilyName,
    string Category,
    bool IsLeadGuest,
    int? AgeAtCheckInYears);

public sealed record PublicHotelBookingRoomInput(IReadOnlyList<PublicHotelBookingGuestInput> Guests);

public sealed record PublicHotelBookingContactInput(string? Email, string? Phone);

public sealed record PublicHotelBookingInitiationRequest(
    Guid PlaceId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    PublicHotelBookingContactInput Contact,
    IReadOnlyList<PublicHotelBookingRoomInput> Rooms,
    string? IdempotencyKey);

public sealed record PublicHotelBookingGuestRead(
    Guid GuestId,
    string GivenName,
    string FamilyName,
    string Category,
    int? AgeAtCheckInYears,
    bool IsLeadGuest);

public sealed record PublicHotelBookingRoomRead(
    Guid RoomReservationId,
    int Ordinal,
    IReadOnlyList<PublicHotelBookingGuestRead> Guests);

public sealed record PublicHotelBookingContactRead(string? Email, string? Phone);

public sealed record PublicHotelBookingMoneyRead(decimal Amount, string CurrencyCode);

public sealed record PublicHotelBookingCancellationTermRead(
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveUntil,
    decimal PenaltyAmount,
    string CurrencyCode,
    bool CurrentlyExecutable);

public sealed record PublicHotelBookingMonetaryRead(
    Guid SnapshotId,
    string CurrencyCode,
    decimal TotalAmount,
    DateTimeOffset? OfferExpiresAt,
    bool OfferExpired,
    IReadOnlyList<PublicHotelBookingCancellationTermRead> CancellationTerms,
    string? PublicExplanation);

public sealed record PublicHotelBookingHoldRead(string Status, DateTimeOffset? ExpiresAt);

public sealed record PublicHotelBookingReservationRead(
    string Status,
    string? ConfirmationCode);

public sealed record PublicHotelBookingCancellationRead(
    string Status,
    string? FinancialOutcome,
    decimal? PenaltyAmount,
    decimal? RefundAmount,
    string? CurrencyCode);

public sealed record PublicHotelBookingInitiationResponse(
    Guid HotelBookingId,
    string Status,
    string PresentationState,
    string? AccessToken,
    bool AccessTokenIssued,
    bool Confirmed,
    Guid PlaceId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    IReadOnlyList<PublicHotelBookingRoomRead> Rooms);

public sealed record PublicHotelBookingRead(
    Guid HotelBookingId,
    string Status,
    string PresentationState,
    bool Confirmed,
    Guid PlaceId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    PublicHotelBookingContactRead? Contact,
    IReadOnlyList<PublicHotelBookingRoomRead> Rooms,
    PublicHotelBookingMonetaryRead? Monetary,
    PublicHotelBookingHoldRead? Hold,
    PublicHotelBookingReservationRead? Reservation,
    PublicHotelBookingCancellationRead? Cancellation,
    string? PaymentStatus,
    string? RefundStatus,
    bool CancellationAvailable,
    bool RateExpired,
    string? SafeMessage);

public sealed record PublicHotelBookingPaymentRead(
    Guid HotelBookingId,
    string HotelBookingStatus,
    bool HotelBookingConfirmed,
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
    PublicHotelBookingMonetaryRead? Monetary);

public sealed record PublicHotelBookingCancellationCommandResult(
    string Outcome,
    PublicHotelBookingRead Booking);

public interface IPublicHotelBookingInitiationService
{
    Task<PublicHotelBookingInitiationResponse> InitiateAsync(
        PublicHotelBookingInitiationRequest request,
        Guid? actorId,
        CancellationToken cancellationToken = default);
}

public interface IPublicHotelBookingReadService
{
    Task<PublicHotelBookingRead?> GetAuthorizedAsync(
        Guid hotelBookingId,
        string? accessToken,
        Guid? actorId,
        CancellationToken cancellationToken = default);
}

public interface IPublicHotelBookingJourneyService
{
    Task<PublicHotelBookingProgressResult> RequestAvailabilityAsync(
        Guid hotelBookingId,
        string? accessToken,
        Guid? actorId,
        string? idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<PublicHotelBookingProgressResult> RequestRateOfferAsync(
        Guid hotelBookingId,
        string? accessToken,
        Guid? actorId,
        string? idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<PublicHotelBookingCancellationCommandResult?> RequestCancellationAsync(
        Guid hotelBookingId,
        string? accessToken,
        Guid? actorId,
        string? idempotencyKey,
        CancellationToken cancellationToken = default);
}

public sealed record PublicHotelBookingProgressResult(
    PublicHotelBookingJourneyStatus Status,
    PublicHotelBookingRead Booking);

public enum PublicHotelBookingJourneyStatus
{
    Completed = 0,
    SourceUnavailable = 1,
    Ineligible = 2,
}

public interface IHotelPlaceCatalogLookup
{
    Task<bool> IsActiveHotelPlaceAsync(
        Guid placeId,
        CancellationToken cancellationToken = default);
}

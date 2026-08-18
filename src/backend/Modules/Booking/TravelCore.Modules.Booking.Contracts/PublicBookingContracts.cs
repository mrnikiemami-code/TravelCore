namespace TravelCore.Modules.Booking.Contracts;

public sealed record PublicBookingPassengerInput(string GivenName, string FamilyName, string Category);

public sealed record PublicBookingContactInput(string? DisplayName, string? Email, string? Phone);

public sealed record PublicBookingInitiationRequest(
    Guid TourDepartureId,
    PublicBookingContactInput Contact,
    IReadOnlyList<PublicBookingPassengerInput> Passengers,
    string? IdempotencyKey,
    string? SourceKind);

public sealed record PublicBookingMoneyRead(decimal Amount, string CurrencyCode);

public sealed record PublicBookingMonetaryComponentRead(
    string Kind,
    PublicBookingMoneyRead Money,
    int SortOrder,
    string? Code,
    string? Label);

public sealed record PublicBookingMonetaryRead(
    Guid QuoteId,
    Guid SourcePriceId,
    string Currency,
    decimal TotalAmount,
    DateTimeOffset QuoteExpiresAt,
    IReadOnlyList<PublicBookingMonetaryComponentRead> Components);

public sealed record PublicBookingPassengerRead(
    Guid PassengerId,
    string GivenName,
    string FamilyName,
    string Category,
    int Sequence);

public sealed record PublicBookingContactRead(string? DisplayName, string? Email, string? Phone);

public sealed record PublicBookingHoldRead(string Status, DateTimeOffset ExpiresAt, int SeatCount);

public sealed record PublicBookingInitiationResponse(
    Guid BookingId,
    string Status,
    string SourceKind,
    Guid TourDepartureId,
    string? AccessToken,
    bool AccessTokenIssued,
    bool Confirmed,
    PublicBookingMonetaryRead? Monetary,
    PublicBookingHoldRead? Hold,
    IReadOnlyList<PublicBookingPassengerRead> Passengers);

public sealed record PublicBookingRead(
    Guid BookingId,
    string Status,
    string SourceKind,
    Guid TourDepartureId,
    bool Confirmed,
    PublicBookingContactRead? Contact,
    IReadOnlyList<PublicBookingPassengerRead> Passengers,
    PublicBookingMonetaryRead? Monetary,
    PublicBookingHoldRead? Hold);

public interface IPublicBookingInitiationService
{
    Task<PublicBookingInitiationResponse> InitiateAsync(
        PublicBookingInitiationRequest request,
        Guid? actorId,
        CancellationToken cancellationToken = default);
}

public interface IPublicBookingReadService
{
    Task<PublicBookingRead?> GetAuthorizedAsync(
        Guid bookingId,
        string? accessToken,
        Guid? actorId,
        CancellationToken cancellationToken = default);
}

public sealed record PublicBookingPaymentRead(
    Guid BookingId,
    string BookingStatus,
    bool BookingConfirmed,
    Guid PaymentId,
    string PaymentStatus,
    decimal? Amount,
    string? CurrencyCode,
    bool ProviderInitiationPossible,
    string? LatestAttemptStatus,
    string? RefundStatus,
    string SafeAction,
    string? RedirectUri);

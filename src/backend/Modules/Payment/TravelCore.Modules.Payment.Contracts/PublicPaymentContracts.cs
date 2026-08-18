namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Booking-scoped public Payment read/initiation (TC-P20-T007 / P20-R7).
/// Not generic Payment CRUD. Amount/currency/success are never client-authoritative.
/// </summary>
public static class PublicPaymentCompositionBoundary
{
    public const string InitiationRoute = "/api/booking/public/{bookingId}/payment/initiation";
    public const string StatusRoute = "/api/booking/public/{bookingId}/payment";
    public const string BookingIdIsNotAuthorization = "BookingId != authorization";
    public const string PaymentIdIsNotAccessCredential = "PaymentId != Access Credential";
    public const string BrowserReturnIsNotPaymentSuccess = "BrowserReturn != PaymentSuccess";
    public const string PublicPaymentIsNotCrud = "Public Payment != CRUD";
    public const bool PublicRefundApiImplemented = false;
    public const bool PublicPaymentListImplemented = false;
    public const bool GenericPaymentLookupImplemented = false;
    public const bool ClientAmountAuthorityImplemented = false;
    public const bool ClientSuccessAuthorityImplemented = false;
    public const bool CardCollectionImplemented = false;
}

public sealed record PublicPaymentInitiationRequest(string? IdempotencyKey);

public sealed record PublicPaymentRead(
    Guid PaymentId,
    string PaymentStatus,
    decimal? Amount,
    string? CurrencyCode,
    bool ProviderInitiationPossible,
    string? LatestAttemptStatus,
    string? RefundStatus,
    string SafeAction,
    string? RedirectUri);

public interface IPublicBookingPaymentService
{
    Task<PublicPaymentRead> GetByBookingIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task<PublicPaymentCommandResult> InitiateForBookingAsync(
        Guid bookingId,
        string? idempotencyKey,
        CancellationToken cancellationToken = default);
}

public interface IPublicHotelBookingPaymentService
{
    Task<PublicPaymentRead> GetByHotelBookingIdAsync(
        Guid hotelBookingId,
        CancellationToken cancellationToken = default);

    Task<PublicPaymentCommandResult> InitiateForHotelBookingAsync(
        Guid hotelBookingId,
        string? idempotencyKey,
        CancellationToken cancellationToken = default);
}

public sealed record PublicPaymentCommandResult(
    PublicPaymentRead Payment,
    PublicPaymentCommandStatus Status);

public enum PublicPaymentCommandStatus
{
    Completed = 0,
    ProviderUnavailable = 1,
    BookingIneligible = 2,
}

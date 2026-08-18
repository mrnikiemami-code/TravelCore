namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Trusted server-side initiation payload. Amount/currency are not client-authored (P20-R3).
/// Monetary comparison against Booking obligation remains P20-R5.
/// </summary>
public sealed record PaymentInitiationRequest(
    Guid PaymentId,
    Guid PaymentAttemptId,
    Guid BookingId,
    ProviderKey ProviderKey,
    decimal Amount,
    string CurrencyCode);

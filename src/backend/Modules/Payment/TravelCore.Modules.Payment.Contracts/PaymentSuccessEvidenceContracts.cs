namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Payment-owned read contract for Booking confirmation orchestration (P20-R5).
/// </summary>
public sealed record PaymentSuccessEvidenceRead(
    Guid PaymentId,
    Guid BookingId,
    string PaymentStatus,
    decimal Amount,
    string CurrencyCode,
    bool IsAuthoritativeSuccess);

public interface IPaymentSuccessEvidenceQuery
{
    Task<PaymentSuccessEvidenceRead?> GetByBookingIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default);
}

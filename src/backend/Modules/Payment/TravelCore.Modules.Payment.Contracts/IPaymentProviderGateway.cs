namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Provider-neutral Payment gateway port. Provider-specific types stay in adapters (P20-R3).
/// </summary>
public interface IPaymentProviderGateway
{
    ProviderKey Key { get; }

    Task<PaymentInitiationResult> InitiatePaymentAsync(
        PaymentInitiationRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentVerificationResult> VerifyPaymentAsync(
        PaymentVerificationRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentVerificationResult> QueryPaymentStatusAsync(
        PaymentVerificationRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentCallbackVerification> VerifyCallbackAsync(
        PaymentCallbackEnvelope envelope,
        CancellationToken cancellationToken = default);

    Task<PaymentInitiationResult> InitiateRefundAsync(
        RefundInitiationRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentVerificationResult> VerifyRefundAsync(
        PaymentVerificationRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentVerificationResult> QueryRefundStatusAsync(
        PaymentVerificationRequest request,
        CancellationToken cancellationToken = default);
}

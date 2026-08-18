using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Payment.UnitTests.Fakes;

/// <summary>
/// Test-only provider adapter. Not a production Payment truth source (P20-R3).
/// </summary>
internal sealed class FakePaymentProviderGateway : IPaymentProviderGateway
{
    public const string VerifiedHeaderName = "X-TravelCore-Provider-Verified";

    public FakePaymentProviderGateway(ProviderKey key)
    {
        Key = key;
    }

    public ProviderKey Key { get; }

    public PaymentInitiationOutcome NextInitiation { get; set; } = PaymentInitiationOutcome.Initiated;

    public ProviderVerificationOutcome NextVerification { get; set; } = ProviderVerificationOutcome.Succeeded;

    public bool ThrowOnInitiate { get; set; }

    public ProviderRequestReference RequestReference { get; set; } = new("req-test-1");

    public ProviderTransactionReference TransactionReference { get; set; } = new("txn-test-1");

    public Uri RedirectUri { get; set; } = new("https://example.test/redirect");

    public Task<PaymentInitiationResult> InitiatePaymentAsync(
        PaymentInitiationRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (ThrowOnInitiate)
        {
            throw new InvalidOperationException("Simulated provider network failure.");
        }

        return Task.FromResult(new PaymentInitiationResult
        {
            Outcome = NextInitiation,
            ProviderKey = Key,
            RequestReference = NextInitiation == PaymentInitiationOutcome.DefinitiveFailure
                ? null
                : RequestReference,
            TransactionReference = NextInitiation == PaymentInitiationOutcome.Initiated
                ? TransactionReference
                : null,
            RedirectUri = NextInitiation == PaymentInitiationOutcome.Initiated ? RedirectUri : null,
        });
    }

    public Task<PaymentVerificationResult> VerifyPaymentAsync(
        PaymentVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(CreateVerification());
    }

    public Task<PaymentVerificationResult> QueryPaymentStatusAsync(
        PaymentVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        return VerifyPaymentAsync(request, cancellationToken);
    }

    public Task<PaymentCallbackVerification> VerifyCallbackAsync(
        PaymentCallbackEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!envelope.Headers.TryGetValue(VerifiedHeaderName, out var verified)
            || !string.Equals(verified, "true", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(PaymentCallbackVerification.Unverified());
        }

        return Task.FromResult(PaymentCallbackVerification.Verified(CreateVerification()));
    }

    private PaymentVerificationResult CreateVerification() => new()
    {
        Outcome = NextVerification,
        ProviderKey = Key,
        RequestReference = RequestReference,
        TransactionReference = TransactionReference,
    };
}

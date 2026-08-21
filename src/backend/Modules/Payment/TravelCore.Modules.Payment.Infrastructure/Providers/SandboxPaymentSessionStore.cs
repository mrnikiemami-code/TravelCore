using System.Collections.Concurrent;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Payment.Infrastructure.Providers;

/// <summary>
/// Process-local sandbox session map for status query smoke only (not production truth).
/// </summary>
internal sealed class SandboxPaymentSessionStore
{
    private readonly ConcurrentDictionary<string, SandboxPaymentSession> _sessions = new(StringComparer.Ordinal);

    public void TrackInitiated(SandboxPaymentSession session) =>
        _sessions[session.RequestReference.Value] = session;

    public bool TryGet(ProviderRequestReference requestReference, out SandboxPaymentSession session) =>
        _sessions.TryGetValue(requestReference.Value, out session!);

    public void RecordOutcome(
        ProviderRequestReference requestReference,
        ProviderVerificationOutcome outcome,
        ProviderTransactionReference? transactionReference)
    {
        _sessions.AddOrUpdate(
            requestReference.Value,
            _ => new SandboxPaymentSession(
                requestReference,
                transactionReference,
                Amount: null,
                CurrencyCode: null,
                outcome),
            (_, existing) => existing with
            {
                Outcome = outcome,
                TransactionReference = transactionReference ?? existing.TransactionReference,
            });
    }
}

internal sealed record SandboxPaymentSession(
    ProviderRequestReference RequestReference,
    ProviderTransactionReference? TransactionReference,
    decimal? Amount,
    string? CurrencyCode,
    ProviderVerificationOutcome Outcome);

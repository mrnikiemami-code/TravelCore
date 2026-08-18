using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

/// <summary>
/// Applies trusted, already-verified provider refund outcomes. Never trusts client/browser flags (P20-R6).
/// </summary>
internal static class VerifiedRefundOutcomeApplier
{
    public static VerificationApplyStatus ApplyVerification(
        Refund refund,
        RefundAttempt attempt,
        PaymentVerificationResult result,
        Instant now)
    {
        ArgumentNullException.ThrowIfNull(refund);
        ArgumentNullException.ThrowIfNull(attempt);
        ArgumentNullException.ThrowIfNull(result);

        switch (result.Outcome)
        {
            case ProviderVerificationOutcome.Succeeded:
                if (result.ReportedAmount is not decimal reportedAmount)
                {
                    return VerificationApplyStatus.AmountMismatch;
                }

                if (reportedAmount != refund.Amount.Amount)
                {
                    return VerificationApplyStatus.AmountMismatch;
                }

                if (string.IsNullOrWhiteSpace(result.ReportedCurrencyCode))
                {
                    return VerificationApplyStatus.CurrencyMismatch;
                }

                if (!string.Equals(result.ReportedCurrencyCode, refund.Amount.Currency.Value, StringComparison.Ordinal))
                {
                    return VerificationApplyStatus.CurrencyMismatch;
                }

                if (attempt.Status == RefundAttemptStatus.Failed)
                {
                    return VerificationApplyStatus.Contradiction;
                }

                if (attempt.Status == RefundAttemptStatus.Succeeded
                    && refund.Status == RefundStatus.Succeeded)
                {
                    return VerificationApplyStatus.Unchanged;
                }

                refund.RecordAuthoritativeRefundSuccess(attempt.Id, now);
                return VerificationApplyStatus.Applied;
            case ProviderVerificationOutcome.Failed:
                if (attempt.Status == RefundAttemptStatus.Succeeded)
                {
                    return VerificationApplyStatus.Contradiction;
                }

                if (attempt.Status == RefundAttemptStatus.Failed)
                {
                    return VerificationApplyStatus.Unchanged;
                }

                refund.RecordAttemptFailure(attempt.Id, now);
                return VerificationApplyStatus.Applied;
            case ProviderVerificationOutcome.PendingUnknown:
                return VerificationApplyStatus.Unchanged;
            default:
                throw new InvalidOperationException("Unknown provider verification outcome.");
        }
    }

    public static void ApplyInitiation(
        Refund refund,
        RefundAttempt attempt,
        PaymentInitiationResult result,
        Instant now)
    {
        ArgumentNullException.ThrowIfNull(refund);
        ArgumentNullException.ThrowIfNull(attempt);
        ArgumentNullException.ThrowIfNull(result);

        switch (result.Outcome)
        {
            case PaymentInitiationOutcome.Initiated:
                refund.RecordProviderInitiation(
                    attempt.Id,
                    now,
                    result.ProviderKey,
                    result.RequestReference,
                    result.TransactionReference);
                return;
            case PaymentInitiationOutcome.DefinitiveFailure:
                refund.RecordAttemptFailure(attempt.Id, now);
                return;
            case PaymentInitiationOutcome.Unknown:
                refund.RecordAmbiguousProviderInitiation(
                    attempt.Id,
                    now,
                    result.ProviderKey,
                    result.RequestReference,
                    result.TransactionReference);
                return;
            default:
                throw new InvalidOperationException("Unknown provider initiation outcome.");
        }
    }
}

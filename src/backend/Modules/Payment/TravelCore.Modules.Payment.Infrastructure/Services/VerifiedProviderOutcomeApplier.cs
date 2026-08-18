using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

internal enum VerificationApplyStatus
{
    Applied = 1,
    Unchanged = 2,
    Contradiction = 3,
}

/// <summary>
/// Applies trusted, already-verified provider outcomes to Payment. Never trusts client/browser flags (P20-R3).
/// </summary>
internal static class VerifiedProviderOutcomeApplier
{
    public static VerificationApplyStatus ApplyVerification(
        PaymentAggregate payment,
        PaymentAttempt attempt,
        ProviderVerificationOutcome outcome,
        Instant now)
    {
        ArgumentNullException.ThrowIfNull(payment);
        ArgumentNullException.ThrowIfNull(attempt);

        switch (outcome)
        {
            case ProviderVerificationOutcome.Succeeded:
                if (attempt.Status == PaymentAttemptStatus.Failed)
                {
                    return VerificationApplyStatus.Contradiction;
                }

                if (attempt.Status == PaymentAttemptStatus.Succeeded
                    && payment.Status == PaymentStatus.Succeeded)
                {
                    return VerificationApplyStatus.Unchanged;
                }

                payment.RecordAuthoritativeCollectionSuccess(attempt.Id, now);
                return VerificationApplyStatus.Applied;
            case ProviderVerificationOutcome.Failed:
                if (attempt.Status == PaymentAttemptStatus.Succeeded)
                {
                    return VerificationApplyStatus.Contradiction;
                }

                if (attempt.Status == PaymentAttemptStatus.Failed)
                {
                    return VerificationApplyStatus.Unchanged;
                }

                payment.RecordAttemptFailure(attempt.Id, now);
                return VerificationApplyStatus.Applied;
            case ProviderVerificationOutcome.PendingUnknown:
                return VerificationApplyStatus.Unchanged;
            default:
                throw new InvalidOperationException("Unknown provider verification outcome.");
        }
    }

    public static void ApplyInitiation(
        PaymentAggregate payment,
        PaymentAttempt attempt,
        PaymentInitiationResult result,
        Instant now)
    {
        ArgumentNullException.ThrowIfNull(payment);
        ArgumentNullException.ThrowIfNull(attempt);
        ArgumentNullException.ThrowIfNull(result);

        switch (result.Outcome)
        {
            case PaymentInitiationOutcome.Initiated:
                payment.RecordProviderInitiation(
                    attempt.Id,
                    now,
                    result.ProviderKey,
                    result.RequestReference,
                    result.TransactionReference);
                return;
            case PaymentInitiationOutcome.DefinitiveFailure:
                payment.RecordAttemptFailure(attempt.Id, now);
                return;
            case PaymentInitiationOutcome.Unknown:
                payment.RecordAmbiguousProviderInitiation(
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

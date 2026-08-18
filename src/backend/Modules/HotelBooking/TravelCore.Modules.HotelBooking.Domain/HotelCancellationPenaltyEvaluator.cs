using NodaTime;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// Selects the exact applicable snapshotted penalty rule at RequestedAt Instant.
/// Live supplier policy and Place catalog changes are not authority.
/// </summary>
public static class HotelCancellationPenaltyEvaluator
{
    public static HotelCancellationPenaltyEvaluation Evaluate(
        HotelCancellationPolicySnapshot policy,
        HotelBookingMonetarySnapshot monetary,
        Instant requestedAt)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(monetary);
        if (requestedAt == default)
        {
            throw new ArgumentException("RequestedAt cannot be default.", nameof(requestedAt));
        }

        var matching = policy.Rules
            .Where(rule => Applies(rule, requestedAt))
            .ToArray();
        if (matching.Length != 1)
        {
            return HotelCancellationPenaltyEvaluation.NoDeterministicRule();
        }

        var penalty = matching[0].Penalty;
        ArgumentNullException.ThrowIfNull(penalty);
        if (penalty.Currency != monetary.CurrencyCode)
        {
            throw new InvalidOperationException(
                "Penalty CurrencyCode must match HotelBookingMonetarySnapshot.CurrencyCode.");
        }

        var total = monetary.Total;
        if (penalty.Amount == 0m)
        {
            return HotelCancellationPenaltyEvaluation.FullRefund(total);
        }

        if (penalty.Equals(total))
        {
            return HotelCancellationPenaltyEvaluation.NoRefund(total);
        }

        if (penalty.Amount > 0m && penalty < total)
        {
            return HotelCancellationPenaltyEvaluation.PartialRefundRequiredUnsupported(
                penalty,
                total.Subtract(penalty));
        }

        return HotelCancellationPenaltyEvaluation.NoDeterministicRule();
    }

    private static bool Applies(HotelCancellationPenaltyRule rule, Instant requestedAt)
    {
        if (requestedAt < rule.EffectiveFrom)
        {
            return false;
        }

        return rule.EffectiveUntil is null || requestedAt < rule.EffectiveUntil.Value;
    }
}

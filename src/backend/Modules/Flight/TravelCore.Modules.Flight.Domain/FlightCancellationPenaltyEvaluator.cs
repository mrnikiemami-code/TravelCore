using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Evaluates accepted fare-rule CancelPenalty against FlightBookingMonetarySnapshot.Total.
/// Live supplier quote is compared separately; snapshots are never mutated.
/// </summary>
public static class FlightCancellationPenaltyEvaluator
{
    public static FlightCancellationPenaltyEvaluation Evaluate(
        FlightFareRulesSnapshot fareRules,
        FlightBookingMonetarySnapshot monetary)
    {
        ArgumentNullException.ThrowIfNull(fareRules);
        ArgumentNullException.ThrowIfNull(monetary);
        return EvaluatePenalty(fareRules.CancelPenalty, monetary.Total, fareRules.PartialRefundRequired);
    }

    public static FlightCancellationPenaltyEvaluation EvaluatePenalty(
        MoneyValue? penalty,
        MoneyValue total,
        bool partialRefundRequired)
    {
        ArgumentNullException.ThrowIfNull(total);
        if (partialRefundRequired)
        {
            var reported = penalty ?? new MoneyValue(0m, total.Currency);
            if (reported.Currency != total.Currency)
            {
                return FlightCancellationPenaltyEvaluation.NoDeterministicRule();
            }

            var refund = reported.Amount >= total.Amount
                ? new MoneyValue(0m, total.Currency)
                : total.Subtract(reported);
            return FlightCancellationPenaltyEvaluation.PartialRefundRequiredUnsupported(reported, refund);
        }

        if (penalty is null)
        {
            return FlightCancellationPenaltyEvaluation.NoDeterministicRule();
        }

        if (penalty.Currency != total.Currency)
        {
            return FlightCancellationPenaltyEvaluation.NoDeterministicRule();
        }

        if (penalty.Amount == 0m)
        {
            return FlightCancellationPenaltyEvaluation.FullRefund(total);
        }

        if (penalty.Equals(total))
        {
            return FlightCancellationPenaltyEvaluation.NoRefund(total);
        }

        if (penalty.Amount > 0m && penalty < total)
        {
            return FlightCancellationPenaltyEvaluation.PartialRefundRequiredUnsupported(
                penalty,
                total.Subtract(penalty));
        }

        return FlightCancellationPenaltyEvaluation.NoDeterministicRule();
    }
}

using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Result of evaluating accepted FlightFareRulesSnapshot.CancelPenalty against Total.
/// Not FlightBookingStatus and not cancellation execution.
/// </summary>
public enum FlightCancellationPenaltyEvaluationKind
{
    FullRefund = 1,
    NoRefund = 2,
    PartialRefundRequiredUnsupported = 3,
    NoDeterministicRule = 4,
}

public sealed class FlightCancellationPenaltyEvaluation
{
    private FlightCancellationPenaltyEvaluation(
        FlightCancellationPenaltyEvaluationKind kind,
        MoneyValue? penalty,
        MoneyValue? refundAmount)
    {
        Kind = kind;
        Penalty = penalty;
        RefundAmount = refundAmount;
    }

    public FlightCancellationPenaltyEvaluationKind Kind { get; }

    public MoneyValue? Penalty { get; }

    public MoneyValue? RefundAmount { get; }

    public bool IsExecutable =>
        Kind is FlightCancellationPenaltyEvaluationKind.FullRefund
            or FlightCancellationPenaltyEvaluationKind.NoRefund;

    public static FlightCancellationPenaltyEvaluation FullRefund(MoneyValue total) =>
        new(FlightCancellationPenaltyEvaluationKind.FullRefund, Zero(total), total);

    public static FlightCancellationPenaltyEvaluation NoRefund(MoneyValue total) =>
        new(FlightCancellationPenaltyEvaluationKind.NoRefund, total, Zero(total));

    public static FlightCancellationPenaltyEvaluation PartialRefundRequiredUnsupported(
        MoneyValue penalty,
        MoneyValue refundAmount) =>
        new(FlightCancellationPenaltyEvaluationKind.PartialRefundRequiredUnsupported, penalty, refundAmount);

    public static FlightCancellationPenaltyEvaluation NoDeterministicRule() =>
        new(FlightCancellationPenaltyEvaluationKind.NoDeterministicRule, null, null);

    private static MoneyValue Zero(MoneyValue total) => new(0m, total.Currency);
}

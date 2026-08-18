using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// Result of evaluating an immutable HotelCancellationPolicySnapshot at RequestedAt.
/// Not HotelBookingStatus and not cancellation execution.
/// </summary>
public enum HotelCancellationPenaltyEvaluationKind
{
    FullRefund = 1,
    NoRefund = 2,
    PartialRefundRequiredUnsupported = 3,
    NoDeterministicRule = 4,
}

public sealed class HotelCancellationPenaltyEvaluation
{
    private HotelCancellationPenaltyEvaluation(
        HotelCancellationPenaltyEvaluationKind kind,
        MoneyValue? penalty,
        MoneyValue? refundAmount)
    {
        Kind = kind;
        Penalty = penalty;
        RefundAmount = refundAmount;
    }

    public HotelCancellationPenaltyEvaluationKind Kind { get; }

    public MoneyValue? Penalty { get; }

    public MoneyValue? RefundAmount { get; }

    public bool IsExecutable =>
        Kind is HotelCancellationPenaltyEvaluationKind.FullRefund
            or HotelCancellationPenaltyEvaluationKind.NoRefund;

    public static HotelCancellationPenaltyEvaluation FullRefund(MoneyValue total) =>
        new(HotelCancellationPenaltyEvaluationKind.FullRefund, Zero(total), total);

    public static HotelCancellationPenaltyEvaluation NoRefund(MoneyValue total) =>
        new(HotelCancellationPenaltyEvaluationKind.NoRefund, total, Zero(total));

    public static HotelCancellationPenaltyEvaluation PartialRefundRequiredUnsupported(
        MoneyValue penalty,
        MoneyValue refundAmount) =>
        new(HotelCancellationPenaltyEvaluationKind.PartialRefundRequiredUnsupported, penalty, refundAmount);

    public static HotelCancellationPenaltyEvaluation NoDeterministicRule() =>
        new(HotelCancellationPenaltyEvaluationKind.NoDeterministicRule, null, null);

    private static MoneyValue Zero(MoneyValue total) => new(0m, total.Currency);
}

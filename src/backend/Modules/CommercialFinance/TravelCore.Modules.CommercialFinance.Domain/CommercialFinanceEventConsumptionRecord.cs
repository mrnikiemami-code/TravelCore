using NodaTime;

namespace TravelCore.Modules.CommercialFinance.Domain;

/// <summary>
/// Strict idempotent consumption record for upstream source events (P39 Q12 derived).
/// </summary>
public sealed class CommercialFinanceEventConsumptionRecord
{
    public const int SourceEventKeyMaxLength = 256;

    private CommercialFinanceEventConsumptionRecord()
    {
        SourceEventKey = null!;
    }

    private CommercialFinanceEventConsumptionRecord(
        CommercialFinanceEventSourceKind sourceKind,
        string sourceEventKey,
        CommercialObligationId obligationId,
        Instant consumedAt)
    {
        SourceKind = sourceKind;
        SourceEventKey = sourceEventKey;
        ObligationId = obligationId;
        ConsumedAt = consumedAt;
    }

    public CommercialFinanceEventSourceKind SourceKind { get; private set; }

    public string SourceEventKey { get; private set; }

    public CommercialObligationId ObligationId { get; private set; }

    public Instant ConsumedAt { get; private set; }

    public static CommercialFinanceEventConsumptionRecord Create(
        CommercialFinanceEventSourceKind sourceKind,
        string sourceEventKey,
        CommercialObligationId obligationId,
        Instant consumedAt)
    {
        var key = CommercialObligation.NormalizeSourceEventKey(sourceEventKey);
        if (consumedAt == default)
        {
            throw new ArgumentException("ConsumedAt cannot be default.", nameof(consumedAt));
        }

        return new CommercialFinanceEventConsumptionRecord(sourceKind, key, obligationId, consumedAt);
    }
}

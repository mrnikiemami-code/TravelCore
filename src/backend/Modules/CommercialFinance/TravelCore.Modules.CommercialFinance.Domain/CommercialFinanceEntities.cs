using NodaTime;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.CommercialFinance.Domain;

/// <summary>
/// Agency-specific commission agreement terms (structure only — no formula fields).
/// </summary>
public sealed class CommissionAgreement
{
    private CommissionAgreement()
    {
    }

    private CommissionAgreement(
        CommissionAgreementId id,
        CommercialFinanceAgencyProfileId agencyProfileId,
        CommercialFinanceMarketPolicy marketPolicy,
        CommissionAgreementStatus status,
        Instant effectiveFrom,
        Instant? effectiveTo,
        Instant createdAt,
        Instant updatedAt)
    {
        Id = id;
        AgencyProfileId = agencyProfileId;
        MarketPolicy = marketPolicy;
        Status = status;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public CommissionAgreementId Id { get; private set; }

    public CommercialFinanceAgencyProfileId AgencyProfileId { get; private set; }

    public CommercialFinanceMarketPolicy MarketPolicy { get; private set; }

    public CommissionAgreementStatus Status { get; private set; }

    public Instant EffectiveFrom { get; private set; }

    public Instant? EffectiveTo { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public static CommissionAgreement Create(
        CommissionAgreementId id,
        CommercialFinanceAgencyProfileId agencyProfileId,
        CommercialFinanceMarketPolicy marketPolicy,
        Instant effectiveFrom,
        Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        return new CommissionAgreement(
            id,
            agencyProfileId,
            marketPolicy,
            CommissionAgreementStatus.Draft,
            effectiveFrom,
            effectiveTo: null,
            createdAt: now,
            updatedAt: now);
    }
}

/// <summary>
/// Offer-level commission override context — logical agency_offer_id only; does not mutate AgencyOffer.
/// </summary>
public sealed class AgencyOfferCommissionOverride
{
    private AgencyOfferCommissionOverride()
    {
        Notes = null!;
    }

    private AgencyOfferCommissionOverride(
        AgencyOfferCommissionOverrideId id,
        CommercialFinanceAgencyOfferId agencyOfferId,
        CommissionAgreementId commissionAgreementId,
        AgencyOfferCommissionOverrideStatus status,
        string? notes,
        Instant createdAt,
        Instant updatedAt)
    {
        Id = id;
        AgencyOfferId = agencyOfferId;
        CommissionAgreementId = commissionAgreementId;
        Status = status;
        Notes = notes;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public AgencyOfferCommissionOverrideId Id { get; private set; }

    public CommercialFinanceAgencyOfferId AgencyOfferId { get; private set; }

    public CommissionAgreementId CommissionAgreementId { get; private set; }

    public AgencyOfferCommissionOverrideStatus Status { get; private set; }

    public string? Notes { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public const int NotesMaxLength = 2000;

    public static AgencyOfferCommissionOverride Create(
        AgencyOfferCommissionOverrideId id,
        CommercialFinanceAgencyOfferId agencyOfferId,
        CommissionAgreementId commissionAgreementId,
        string? notes,
        Instant now)
    {
        if (notes is not null && notes.Length > NotesMaxLength)
        {
            throw new ArgumentException($"Notes cannot exceed {NotesMaxLength} characters.", nameof(notes));
        }

        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        return new AgencyOfferCommissionOverride(
            id,
            agencyOfferId,
            commissionAgreementId,
            AgencyOfferCommissionOverrideStatus.Active,
            notes?.Trim(),
            createdAt: now,
            updatedAt: now);
    }
}

/// <summary>
/// Single commercial duty bridge between evidence and settlement (P39-T003).
/// Optional amount snapshot when supplied — not authoritative Pricing/Quote.
/// </summary>
public sealed class CommercialObligation
{
    private CommercialObligation()
    {
        SourceEventKey = null!;
    }

    private CommercialObligation(
        CommercialObligationId id,
        CommercialFinanceAgencyProfileId agencyProfileId,
        CommercialFinanceAgencyOfferId? agencyOfferId,
        CommercialFinanceBookingId bookingId,
        CommercialFinancePaymentId? paymentId,
        CommercialObligationLifecycleState lifecycleState,
        string sourceEventKey,
        MoneyValue? amountSnapshot,
        string? evidenceSnapshotHash,
        Instant createdAt,
        Instant updatedAt,
        Instant stateChangedAt)
    {
        Id = id;
        AgencyProfileId = agencyProfileId;
        AgencyOfferId = agencyOfferId;
        BookingId = bookingId;
        PaymentId = paymentId;
        LifecycleState = lifecycleState;
        SourceEventKey = sourceEventKey;
        AmountSnapshot = amountSnapshot;
        EvidenceSnapshotHash = evidenceSnapshotHash;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        StateChangedAt = stateChangedAt;
    }

    public CommercialObligationId Id { get; private set; }

    public CommercialFinanceAgencyProfileId AgencyProfileId { get; private set; }

    public CommercialFinanceAgencyOfferId? AgencyOfferId { get; private set; }

    public CommercialFinanceBookingId BookingId { get; private set; }

    public CommercialFinancePaymentId? PaymentId { get; private set; }

    public CommercialObligationLifecycleState LifecycleState { get; private set; }

    public string SourceEventKey { get; private set; }

    public MoneyValue? AmountSnapshot { get; private set; }

    public string? EvidenceSnapshotHash { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public Instant StateChangedAt { get; private set; }

    public const int SourceEventKeyMaxLength = 256;
    public const int EvidenceSnapshotHashMaxLength = 128;

    public static CommercialObligation Create(
        CommercialObligationId id,
        CommercialFinanceAgencyProfileId agencyProfileId,
        CommercialFinanceAgencyOfferId? agencyOfferId,
        CommercialFinanceBookingId bookingId,
        CommercialFinancePaymentId? paymentId,
        string sourceEventKey,
        MoneyValue? amountSnapshot,
        string? evidenceSnapshotHash,
        Instant now)
    {
        var key = NormalizeSourceEventKey(sourceEventKey);
        ValidateOptionalSnapshot(amountSnapshot, evidenceSnapshotHash);

        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        return new CommercialObligation(
            id,
            agencyProfileId,
            agencyOfferId,
            bookingId,
            paymentId,
            CommercialObligationLifecycleState.Created,
            key,
            amountSnapshot,
            evidenceSnapshotHash?.Trim(),
            createdAt: now,
            updatedAt: now,
            stateChangedAt: now);
    }

    public void TransitionTo(CommercialObligationLifecycleState target, Instant now)
    {
        CommercialObligationLifecycleRules.EnsureCanTransition(LifecycleState, target);
        if (now == default)
        {
            throw new ArgumentException("StateChangedAt cannot be default.", nameof(now));
        }

        LifecycleState = target;
        UpdatedAt = now;
        StateChangedAt = now;
    }

    public static string NormalizeSourceEventKey(string sourceEventKey)
    {
        if (string.IsNullOrWhiteSpace(sourceEventKey))
        {
            throw new ArgumentException("Source event key is required.", nameof(sourceEventKey));
        }

        var trimmed = sourceEventKey.Trim();
        if (trimmed.Length > SourceEventKeyMaxLength)
        {
            throw new ArgumentException(
                $"Source event key cannot exceed {SourceEventKeyMaxLength} characters.",
                nameof(sourceEventKey));
        }

        return trimmed;
    }

    private static void ValidateOptionalSnapshot(MoneyValue? amountSnapshot, string? evidenceSnapshotHash)
    {
        if (amountSnapshot is null)
        {
            return;
        }

        if (amountSnapshot.Amount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amountSnapshot),
                "Amount snapshot cannot be negative in T006 skeleton.");
        }

        if (evidenceSnapshotHash is not null
            && evidenceSnapshotHash.Trim().Length > EvidenceSnapshotHashMaxLength)
        {
            throw new ArgumentException(
                $"Evidence snapshot hash cannot exceed {EvidenceSnapshotHashMaxLength} characters.",
                nameof(evidenceSnapshotHash));
        }
    }
}

/// <summary>Time-bounded grouping window for obligations (P39-T002).</summary>
public sealed class SettlementPeriod
{
    private SettlementPeriod()
    {
    }

    private SettlementPeriod(
        SettlementPeriodId id,
        CommercialFinanceAgencyProfileId agencyProfileId,
        SettlementPeriodStatus status,
        Instant periodStart,
        Instant periodEnd,
        Instant createdAt,
        Instant updatedAt)
    {
        Id = id;
        AgencyProfileId = agencyProfileId;
        Status = status;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public SettlementPeriodId Id { get; private set; }

    public CommercialFinanceAgencyProfileId AgencyProfileId { get; private set; }

    public SettlementPeriodStatus Status { get; private set; }

    public Instant PeriodStart { get; private set; }

    public Instant PeriodEnd { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public static SettlementPeriod CreateOpen(
        SettlementPeriodId id,
        CommercialFinanceAgencyProfileId agencyProfileId,
        Instant periodStart,
        Instant periodEnd,
        Instant now)
    {
        if (periodEnd <= periodStart)
        {
            throw new ArgumentException("PeriodEnd must be after PeriodStart.", nameof(periodEnd));
        }

        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        return new SettlementPeriod(
            id,
            agencyProfileId,
            SettlementPeriodStatus.Open,
            periodStart,
            periodEnd,
            createdAt: now,
            updatedAt: now);
    }
}

/// <summary>Aggregated settlement statement for a period (no Payment execution).</summary>
public sealed class SettlementRecord
{
    private SettlementRecord()
    {
    }

    private SettlementRecord(
        SettlementRecordId id,
        SettlementPeriodId settlementPeriodId,
        SettlementRecordStatus status,
        bool approvalRequired,
        Instant? approvedAt,
        Instant createdAt,
        Instant updatedAt)
    {
        Id = id;
        SettlementPeriodId = settlementPeriodId;
        Status = status;
        ApprovalRequired = approvalRequired;
        ApprovedAt = approvedAt;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public SettlementRecordId Id { get; private set; }

    public SettlementPeriodId SettlementPeriodId { get; private set; }

    public SettlementRecordStatus Status { get; private set; }

    public bool ApprovalRequired { get; private set; }

    public Instant? ApprovedAt { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public static SettlementRecord CreateDraft(
        SettlementRecordId id,
        SettlementPeriodId settlementPeriodId,
        bool approvalRequired,
        Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        return new SettlementRecord(
            id,
            settlementPeriodId,
            SettlementRecordStatus.Draft,
            approvalRequired,
            approvedAt: null,
            createdAt: now,
            updatedAt: now);
    }
}

/// <summary>Semi-automated payout instruction draft — no bank/API rails (P39-T005 §9).</summary>
public sealed class PayoutInstruction
{
    private PayoutInstruction()
    {
    }

    private PayoutInstruction(
        PayoutInstructionId id,
        SettlementRecordId settlementRecordId,
        PayoutInstructionStatus status,
        MoneyValue? payoutAmountSnapshot,
        bool approvalRequired,
        Instant? approvedAt,
        Instant createdAt,
        Instant updatedAt)
    {
        Id = id;
        SettlementRecordId = settlementRecordId;
        Status = status;
        PayoutAmountSnapshot = payoutAmountSnapshot;
        ApprovalRequired = approvalRequired;
        ApprovedAt = approvedAt;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public PayoutInstructionId Id { get; private set; }

    public SettlementRecordId SettlementRecordId { get; private set; }

    public PayoutInstructionStatus Status { get; private set; }

    public MoneyValue? PayoutAmountSnapshot { get; private set; }

    public bool ApprovalRequired { get; private set; }

    public Instant? ApprovedAt { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public static PayoutInstruction CreateDraft(
        PayoutInstructionId id,
        SettlementRecordId settlementRecordId,
        MoneyValue? payoutAmountSnapshot,
        bool approvalRequired,
        Instant now)
    {
        if (payoutAmountSnapshot is not null && payoutAmountSnapshot.Amount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(payoutAmountSnapshot),
                "Payout amount snapshot cannot be negative in T006 skeleton.");
        }

        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        return new PayoutInstruction(
            id,
            settlementRecordId,
            PayoutInstructionStatus.Draft,
            payoutAmountSnapshot,
            approvalRequired,
            approvedAt: null,
            createdAt: now,
            updatedAt: now);
    }
}

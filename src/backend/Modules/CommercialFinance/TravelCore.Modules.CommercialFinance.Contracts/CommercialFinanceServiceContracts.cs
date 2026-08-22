namespace TravelCore.Modules.CommercialFinance.Contracts;

/// <summary>Skeleton read port for commission agreements (TC-P39-T006).</summary>
public interface ICommercialFinanceAgreementQuery
{
    Task<IReadOnlyList<CommercialFinanceAgreementSummary>> ListAgreementsAsync(
        Guid? agencyProfileId,
        int take,
        CancellationToken cancellationToken = default);
}

/// <summary>Skeleton read port for commercial obligations (TC-P39-T006).</summary>
public interface ICommercialFinanceObligationQuery
{
    Task<IReadOnlyList<CommercialFinanceObligationSummary>> ListObligationsAsync(
        Guid? agencyProfileId,
        string? lifecycleState,
        int take,
        CancellationToken cancellationToken = default);
}

/// <summary>Skeleton read port for settlement periods/records (TC-P39-T006).</summary>
public interface ICommercialFinanceSettlementQuery
{
    Task<IReadOnlyList<CommercialFinanceSettlementPeriodSummary>> ListSettlementPeriodsAsync(
        Guid? agencyProfileId,
        int take,
        CancellationToken cancellationToken = default);
}

/// <summary>Skeleton read port for payout instructions (TC-P39-T006).</summary>
public interface ICommercialFinancePayoutQuery
{
    Task<IReadOnlyList<CommercialFinancePayoutInstructionSummary>> ListPayoutInstructionsAsync(
        Guid? settlementRecordId,
        int take,
        CancellationToken cancellationToken = default);
}

/// <summary>Read-only evidence port — no upstream mutation (P39-T003).</summary>
public interface ICommercialFinanceEvidencePort
{
    Task<bool> AgencyOfferExistsAsync(Guid agencyOfferId, CancellationToken cancellationToken = default);

    Task<bool> BookingExistsAsync(Guid bookingId, CancellationToken cancellationToken = default);

    Task<bool> PaymentExistsAsync(Guid paymentId, CancellationToken cancellationToken = default);
}

public sealed record CommercialFinanceAgreementSummary(
    Guid AgreementId,
    Guid AgencyProfileId,
    string MarketPolicy,
    string Status);

public sealed record CommercialFinanceObligationSummary(
    Guid ObligationId,
    Guid AgencyProfileId,
    string LifecycleState,
    Guid? BookingId,
    Guid? PaymentId);

public sealed record CommercialFinanceSettlementPeriodSummary(
    Guid SettlementPeriodId,
    Guid AgencyProfileId,
    string Status,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd);

public sealed record CommercialFinancePayoutInstructionSummary(
    Guid PayoutInstructionId,
    Guid SettlementRecordId,
    string Status);

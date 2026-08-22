using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.CommercialFinance.Contracts;

namespace TravelCore.Modules.CommercialFinance.Infrastructure.Services;

/// <summary>Skeleton agreement query — returns persisted rows only; no fake KPI data.</summary>
internal sealed class CommercialFinanceAgreementQuery(CommercialFinanceDbContext db)
    : ICommercialFinanceAgreementQuery
{
    public async Task<IReadOnlyList<CommercialFinanceAgreementSummary>> ListAgreementsAsync(
        Guid? agencyProfileId,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (take is <= 0 or > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(take), "Take must be between 1 and 200.");
        }

        var query = db.CommissionAgreements.AsNoTracking();
        if (agencyProfileId.HasValue)
        {
            query = query.Where(x => x.AgencyProfileId.Value == agencyProfileId.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .Select(x => new CommercialFinanceAgreementSummary(
                x.Id.Value,
                x.AgencyProfileId.Value,
                x.MarketPolicy.ToString(),
                x.Status.ToString()))
            .ToListAsync(cancellationToken);
    }
}

internal sealed class CommercialFinanceObligationQuery(CommercialFinanceDbContext db)
    : ICommercialFinanceObligationQuery
{
    public async Task<IReadOnlyList<CommercialFinanceObligationSummary>> ListObligationsAsync(
        Guid? agencyProfileId,
        string? lifecycleState,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (take is <= 0 or > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(take), "Take must be between 1 and 200.");
        }

        var query = db.CommercialObligations.AsNoTracking();
        if (agencyProfileId.HasValue)
        {
            query = query.Where(x => x.AgencyProfileId.Value == agencyProfileId.Value);
        }

        if (!string.IsNullOrWhiteSpace(lifecycleState))
        {
            if (!Enum.TryParse<Domain.CommercialObligationLifecycleState>(
                    lifecycleState.Trim(),
                    ignoreCase: true,
                    out var parsed))
            {
                throw new ArgumentException($"Unknown lifecycle state '{lifecycleState}'.", nameof(lifecycleState));
            }

            query = query.Where(x => x.LifecycleState == parsed);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .Select(x => new CommercialFinanceObligationSummary(
                x.Id.Value,
                x.AgencyProfileId.Value,
                x.LifecycleState.ToString(),
                x.BookingId.Value,
                x.PaymentId.HasValue ? x.PaymentId.Value.Value : null))
            .ToListAsync(cancellationToken);
    }
}

internal sealed class CommercialFinanceSettlementQuery(CommercialFinanceDbContext db)
    : ICommercialFinanceSettlementQuery
{
    public async Task<IReadOnlyList<CommercialFinanceSettlementPeriodSummary>> ListSettlementPeriodsAsync(
        Guid? agencyProfileId,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (take is <= 0 or > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(take), "Take must be between 1 and 200.");
        }

        var query = db.SettlementPeriods.AsNoTracking();
        if (agencyProfileId.HasValue)
        {
            query = query.Where(x => x.AgencyProfileId.Value == agencyProfileId.Value);
        }

        return await query
            .OrderByDescending(x => x.PeriodStart)
            .Take(take)
            .Select(x => new CommercialFinanceSettlementPeriodSummary(
                x.Id.Value,
                x.AgencyProfileId.Value,
                x.Status.ToString(),
                x.PeriodStart.ToDateTimeOffset(),
                x.PeriodEnd.ToDateTimeOffset()))
            .ToListAsync(cancellationToken);
    }
}

internal sealed class CommercialFinancePayoutQuery(CommercialFinanceDbContext db)
    : ICommercialFinancePayoutQuery
{
    public async Task<IReadOnlyList<CommercialFinancePayoutInstructionSummary>> ListPayoutInstructionsAsync(
        Guid? settlementRecordId,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (take is <= 0 or > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(take), "Take must be between 1 and 200.");
        }

        var query = db.PayoutInstructions.AsNoTracking();
        if (settlementRecordId.HasValue)
        {
            query = query.Where(x => x.SettlementRecordId.Value == settlementRecordId.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .Select(x => new CommercialFinancePayoutInstructionSummary(
                x.Id.Value,
                x.SettlementRecordId.Value,
                x.Status.ToString()))
            .ToListAsync(cancellationToken);
    }
}

/// <summary>Read-only evidence port stub — always false until cross-module evidence adapters are authorized.</summary>
internal sealed class NullCommercialFinanceEvidencePort : ICommercialFinanceEvidencePort
{
    public Task<bool> AgencyOfferExistsAsync(Guid agencyOfferId, CancellationToken cancellationToken = default) =>
        Task.FromResult(false);

    public Task<bool> BookingExistsAsync(Guid bookingId, CancellationToken cancellationToken = default) =>
        Task.FromResult(false);

    public Task<bool> PaymentExistsAsync(Guid paymentId, CancellationToken cancellationToken = default) =>
        Task.FromResult(false);
}

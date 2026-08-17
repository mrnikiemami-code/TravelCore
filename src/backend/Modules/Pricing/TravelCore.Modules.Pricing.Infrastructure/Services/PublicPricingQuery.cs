using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Pricing.Contracts;
using TravelCore.Modules.Pricing.Domain;

namespace TravelCore.Modules.Pricing.Infrastructure.Services;

/// <summary>
/// Public read-only price summary (TC-P12-T008 / P12-R8). Reads Price facts only.
/// </summary>
public sealed class PublicPricingQuery : IPublicPricingQuery
{
    private readonly PricingDbContext _db;

    public PublicPricingQuery(PricingDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    public Task<PublicPriceSummary?> GetByTourDepartureIdAsync(
        Guid tourDepartureId,
        CancellationToken cancellationToken = default)
        => GetSummaryAsync(PublicPricingTargets.TourDeparture, tourDepartureId, cancellationToken);

    public async Task<PublicPriceSummary?> GetSummaryAsync(
        string targetType,
        Guid targetId,
        CancellationToken cancellationToken = default)
    {
        if (targetId == Guid.Empty)
        {
            throw new ArgumentException("TargetId cannot be empty.", nameof(targetId));
        }

        var parsed = PriceTargetType.Parse(targetType);

        var price = await _db.Prices
            .AsNoTracking()
            .Where(x => x.TargetType == parsed && x.TargetId == targetId)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return price is null ? null : Map(price);
    }

    /// <summary>
    /// Maps a Price to the public summary. Occupancy lines are Public-market only
    /// so Agency marketplace rates (P13) do not leak on the public surface.
    /// </summary>
    internal static PublicPriceSummary Map(Price price)
    {
        ArgumentNullException.ThrowIfNull(price);

        var occupancy = price.OccupancyRulesOrdered
            .Where(rule => rule.MarketPriceType == TourMarketPriceType.Public)
            .Select(rule => new PublicOccupancyPriceSummary(
                rule.PassengerCategory.ToString(),
                rule.OccupancyCategory.ToString(),
                new MoneyResponse(rule.Money.Amount, rule.Money.Currency.Value)))
            .ToList();

        return new PublicPriceSummary(
            price.Id.Value,
            price.TargetType.Value,
            price.TargetId,
            price.Currency.Value,
            price.ComponentsOrdered
                .Select(component => new PublicPriceComponentSummary(
                    component.Kind.ToString(),
                    new MoneyResponse(component.Money.Amount, component.Money.Currency.Value)))
                .ToList(),
            occupancy);
    }
}

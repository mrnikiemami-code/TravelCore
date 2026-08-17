using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Pricing.Domain;

/// <summary>
/// Structured occupancy/passenger commercial rule attached to a Price.
/// </summary>
public sealed class PriceOccupancyRule
{
    private PriceOccupancyRule()
    {
        Money = null!;
    }

    private PriceOccupancyRule(
        PriceOccupancyRuleId id,
        PriceId priceId,
        TourMarketPriceType marketPriceType,
        PassengerCategory passengerCategory,
        OccupancyCategory occupancyCategory,
        MoneyValue money,
        int sortOrder)
    {
        Id = id;
        PriceId = priceId;
        MarketPriceType = marketPriceType;
        PassengerCategory = passengerCategory;
        OccupancyCategory = occupancyCategory;
        Money = money;
        SortOrder = sortOrder;
    }

    public PriceOccupancyRuleId Id { get; private set; }

    public PriceId PriceId { get; private set; }

    public TourMarketPriceType MarketPriceType { get; private set; }

    public PassengerCategory PassengerCategory { get; private set; }

    public OccupancyCategory OccupancyCategory { get; private set; }

    public MoneyValue Money { get; private set; }

    public int SortOrder { get; private set; }

    internal static PriceOccupancyRule Create(
        PriceId priceId,
        TourMarketPriceType marketPriceType,
        PassengerCategory passengerCategory,
        OccupancyCategory occupancyCategory,
        MoneyValue money,
        int sortOrder)
    {
        if (priceId.Value == Guid.Empty)
        {
            throw new ArgumentException("PriceId cannot be empty.", nameof(priceId));
        }

        if (!Enum.IsDefined(marketPriceType))
        {
            throw new ArgumentOutOfRangeException(nameof(marketPriceType), marketPriceType, "Unsupported TourMarketPriceType.");
        }

        if (!Enum.IsDefined(passengerCategory))
        {
            throw new ArgumentOutOfRangeException(nameof(passengerCategory), passengerCategory, "Unsupported PassengerCategory.");
        }

        if (!Enum.IsDefined(occupancyCategory))
        {
            throw new ArgumentOutOfRangeException(nameof(occupancyCategory), occupancyCategory, "Unsupported OccupancyCategory.");
        }

        ArgumentNullException.ThrowIfNull(money);
        PricingCurrency.EnsureCanonical(money.Currency);

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, "SortOrder must be >= 0.");
        }

        return new PriceOccupancyRule(
            PriceOccupancyRuleId.New(),
            priceId,
            marketPriceType,
            passengerCategory,
            occupancyCategory,
            money,
            sortOrder);
    }
}

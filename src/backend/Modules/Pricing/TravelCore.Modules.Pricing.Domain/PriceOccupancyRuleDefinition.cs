using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Pricing.Domain;

/// <summary>
/// Factory input for a structured occupancy/passenger pricing rule.
/// </summary>
public sealed record PriceOccupancyRuleDefinition(
    TourMarketPriceType MarketPriceType,
    PassengerCategory PassengerCategory,
    OccupancyCategory OccupancyCategory,
    MoneyValue Money,
    int SortOrder = 0);

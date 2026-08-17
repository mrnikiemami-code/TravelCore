namespace TravelCore.Modules.Pricing.Contracts;

/// <summary>
/// Public read-only pricing contracts (TC-P12-T008 / P12-R8).
/// Price summary facts only — no Quote mutation, Booking, Payment, Checkout, Availability, Reservation, or FX conversion.
/// </summary>
public static class PublicPricingTargets
{
    /// <summary>Initial logical target (Guid only). Pricing stays generic; Tour types are not referenced.</summary>
    public const string TourDeparture = "TourDeparture";
}

public sealed record PublicPriceComponentSummary(string Kind, MoneyResponse Money);

public sealed record PublicOccupancyPriceSummary(
    string PassengerCategory,
    string OccupancyCategory,
    MoneyResponse Money);

/// <summary>
/// Public price summary: authoritative currency, structured components (kind+money), occupancy prices (categories+money).
/// Source money only — no converted display amount.
/// </summary>
public sealed record PublicPriceSummary(
    Guid PriceId,
    string TargetType,
    Guid TargetId,
    string Currency,
    IReadOnlyList<PublicPriceComponentSummary> Components,
    IReadOnlyList<PublicOccupancyPriceSummary> OccupancyPrices);

/// <summary>
/// Public read query for price summary by polymorphic logical target.
/// </summary>
public interface IPublicPricingQuery
{
    Task<PublicPriceSummary?> GetSummaryAsync(
        string targetType,
        Guid targetId,
        CancellationToken cancellationToken = default);

    /// <summary>Thin helper: TargetType = TourDeparture, TargetId = tourDepartureId.</summary>
    Task<PublicPriceSummary?> GetByTourDepartureIdAsync(
        Guid tourDepartureId,
        CancellationToken cancellationToken = default);
}

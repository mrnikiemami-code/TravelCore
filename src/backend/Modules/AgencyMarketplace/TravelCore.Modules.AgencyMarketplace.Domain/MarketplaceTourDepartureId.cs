namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Logical TourDeparture identity readiness for an AgencyOffer (TC-P13-T005 / P13-R5).
/// TourDeparture remains capacity SoR. This is a Guid-only reference — no Tour FK, no seats.
/// </summary>
public readonly record struct MarketplaceTourDepartureId(Guid Value) : IEquatable<MarketplaceTourDepartureId>
{
    public static MarketplaceTourDepartureId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("MarketplaceTourDepartureId cannot be empty.", nameof(value));
        }

        return new MarketplaceTourDepartureId(value);
    }

    public override string ToString() => Value.ToString("D");
}

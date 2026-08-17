namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Logical Party identity used by Agency Marketplace commercial relationship (P13-R1).
/// Party remains the identity SoR (<c>PartyKind.Agency</c>). This is a Guid-only reference —
/// no Party module type, no EF FK, no AgencyProfile (P13-R2), no AgencyOffer (P13-R3).
/// </summary>
public readonly record struct MarketplacePartyId(Guid Value) : IEquatable<MarketplacePartyId>
{
    public static MarketplacePartyId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("MarketplacePartyId cannot be empty.", nameof(value));
        }

        return new MarketplacePartyId(value);
    }

    public override string ToString() => Value.ToString("D");
}

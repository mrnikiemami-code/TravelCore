namespace TravelCore.Modules.AgencyMarketplace.Contracts;

/// <summary>
/// Party logical-reference readiness (TC-P13-T001 / P13-R1).
/// Agency identity stays in Party; Marketplace may later hold a Guid PartyId only.
/// No Party aggregate copy, no Party FK, no AgencyProfile (P13-R2), no Offer (P13-R3).
/// </summary>
public static class AgencyPartyIdentityBoundary
{
    public const string IdentitySourceModule = "Party";
    public const string CommercialLayerModule = "AgencyMarketplace";
}

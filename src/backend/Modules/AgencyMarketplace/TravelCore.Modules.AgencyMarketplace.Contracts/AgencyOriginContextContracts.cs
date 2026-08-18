namespace TravelCore.Modules.AgencyMarketplace.Contracts;

/// <summary>
/// Trusted AgencyProfile / AgencyOffer identity facts for peer consumption (TC-P19-T007).
/// Read-only — not marketplace mutation, not Pricing, not settlement, not a cloned catalog.
/// Contracts stay free of NodaTime and peer module types.
/// </summary>
public sealed record AgencyOriginProfileFacts(
    Guid AgencyProfileId,
    string Status);

public sealed record AgencyOriginOfferFacts(
    Guid AgencyOfferId,
    Guid AgencyProfileId,
    Guid TourProductId,
    Guid? ReferencedTourDepartureId);

public interface IAgencyOriginContextQuery
{
    Task<AgencyOriginProfileFacts?> GetProfileAsync(
        Guid agencyProfileId,
        CancellationToken cancellationToken = default);

    Task<AgencyOriginOfferFacts?> GetOfferAsync(
        Guid agencyOfferId,
        CancellationToken cancellationToken = default);
}

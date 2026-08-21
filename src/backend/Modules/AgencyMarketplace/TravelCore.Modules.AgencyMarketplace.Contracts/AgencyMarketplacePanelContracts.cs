namespace TravelCore.Modules.AgencyMarketplace.Contracts;

/// <summary>
/// Agency Marketplace panel contracts (TC-P13-T006 / P13-R6; publication TC-P13-T007 / P13-R7).
/// Owned by Agency Marketplace — not Tour Admin, not Identity, not SEO. No Booking/Payment/Commission.
/// </summary>
public sealed record AgencyProfilePanelResponse(
    Guid Id,
    Guid PartyId,
    string DisplayName,
    string? Description,
    Guid? LogoMediaAssetId,
    string? PublicEmail,
    string? PublicPhone,
    string? WebsiteUrl,
    bool PublicListingEnabled,
    string Status);

public sealed record UpsertAgencyProfileRequest(
    Guid PartyId,
    string DisplayName,
    string? Description = null,
    Guid? LogoMediaAssetId = null,
    string? PublicEmail = null,
    string? PublicPhone = null,
    string? WebsiteUrl = null,
    bool PublicListingEnabled = false);

public sealed record AgencyOfferPanelResponse(
    Guid Id,
    Guid AgencyProfileId,
    Guid TourProductId,
    Guid? ReferencedTourDepartureId,
    string DepartureScopeMode,
    IReadOnlyList<Guid> DepartureScopeIds,
    string SalesChannel,
    string? TitleOverride,
    string? Highlight,
    string? CommercialNotes,
    bool RequiresManualConfirmation,
    bool ExclusiveListing,
    bool SalesOpen,
    string Status,
    string Visibility,
    string PublicationStatus,
    string CreatedAt,
    string UpdatedAt);

public sealed record CreateAgencyOfferRequest(
    Guid AgencyProfileId,
    Guid TourProductId,
    string? TitleOverride = null,
    string? Highlight = null,
    string? CommercialNotes = null,
    bool RequiresManualConfirmation = false,
    bool ExclusiveListing = false,
    string SalesChannel = "Public",
    string DepartureScopeMode = "All",
    IReadOnlyList<Guid>? DepartureScopeIds = null);

public interface IAgencyMarketplacePanelService
{
    Task<AgencyProfilePanelResponse> UpsertProfileAsync(
        UpsertAgencyProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<AgencyProfilePanelResponse?> GetProfileAsync(
        Guid profileId,
        CancellationToken cancellationToken = default);

    Task<AgencyProfilePanelResponse?> GetProfileByPartyAsync(
        Guid partyId,
        CancellationToken cancellationToken = default);

    Task ActivateProfileAsync(Guid profileId, CancellationToken cancellationToken = default);

    Task<AgencyOfferPanelResponse> CreateOfferAsync(
        CreateAgencyOfferRequest request,
        CancellationToken cancellationToken = default);

    Task<AgencyOfferPanelResponse?> GetOfferAsync(
        Guid offerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AgencyOfferPanelResponse>> ListOffersAsync(
        Guid agencyProfileId,
        CancellationToken cancellationToken = default);

    Task ActivateOfferAsync(Guid offerId, CancellationToken cancellationToken = default);

    Task ListOfferAsync(Guid offerId, CancellationToken cancellationToken = default);

    Task OpenOfferSalesAsync(Guid offerId, CancellationToken cancellationToken = default);

    Task CloseOfferSalesAsync(Guid offerId, CancellationToken cancellationToken = default);

    Task SubmitOfferAsync(Guid offerId, CancellationToken cancellationToken = default);

    Task ApproveOfferAsync(Guid offerId, CancellationToken cancellationToken = default);

    Task RejectOfferAsync(Guid offerId, CancellationToken cancellationToken = default);

    Task PublishOfferAsync(Guid offerId, CancellationToken cancellationToken = default);

    Task UnpublishOfferAsync(Guid offerId, CancellationToken cancellationToken = default);

    Task SuspendOfferAsync(Guid offerId, CancellationToken cancellationToken = default);

    Task RetireOfferAsync(Guid offerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Throws UnauthorizedAccessException when the offer is not owned by the acting agency profile (P38-T007).
    /// </summary>
    Task EnsureOfferOwnedByAgencyAsync(
        Guid offerId,
        Guid agencyProfileId,
        CancellationToken cancellationToken = default);
}

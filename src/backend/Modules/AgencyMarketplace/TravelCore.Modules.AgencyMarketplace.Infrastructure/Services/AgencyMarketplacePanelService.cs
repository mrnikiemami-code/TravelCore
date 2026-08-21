using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.AgencyMarketplace.Contracts;
using TravelCore.Modules.AgencyMarketplace.Domain;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Services;

/// <summary>
/// Agency Marketplace panel operations (TC-P13-T006 / P13-R6). No Booking/Payment/Commission.
/// </summary>
public sealed class AgencyMarketplacePanelService : IAgencyMarketplacePanelService
{
    private readonly AgencyMarketplaceDbContext _db;

    public AgencyMarketplacePanelService(AgencyMarketplaceDbContext db)
    {
        _db = db;
    }

    public async Task<AgencyProfilePanelResponse> UpsertProfileAsync(
        UpsertAgencyProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var partyId = MarketplacePartyId.From(request.PartyId);
        var display = new AgencyDisplayInfo(request.DisplayName, request.Description, request.LogoMediaAssetId);
        var contact = new AgencyContactSettings(request.PublicEmail, request.PublicPhone, request.WebsiteUrl);
        var commercial = new AgencyCommercialSettings(request.PublicListingEnabled);

        var existing = await _db.AgencyProfiles.SingleOrDefaultAsync(x => x.PartyId == partyId, cancellationToken);
        if (existing is null)
        {
            existing = AgencyProfile.Create(partyId, display, contact, commercial);
            _db.AgencyProfiles.Add(existing);
        }
        else
        {
            existing.UpdateDisplay(display);
            existing.UpdateContact(contact);
            existing.UpdateCommercial(commercial);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Map(existing);
    }

    public async Task<AgencyProfilePanelResponse?> GetProfileAsync(
        Guid profileId,
        CancellationToken cancellationToken = default)
    {
        var profile = await _db.AgencyProfiles.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == AgencyProfileId.From(profileId), cancellationToken);
        return profile is null ? null : Map(profile);
    }

    public async Task<AgencyProfilePanelResponse?> GetProfileByPartyAsync(
        Guid partyId,
        CancellationToken cancellationToken = default)
    {
        var profile = await _db.AgencyProfiles.AsNoTracking()
            .SingleOrDefaultAsync(x => x.PartyId == MarketplacePartyId.From(partyId), cancellationToken);
        return profile is null ? null : Map(profile);
    }

    public async Task ActivateProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var profile = await _db.AgencyProfiles
            .SingleOrDefaultAsync(x => x.Id == AgencyProfileId.From(profileId), cancellationToken)
            ?? throw new KeyNotFoundException("AgencyProfile was not found.");
        profile.Activate();
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<AgencyOfferPanelResponse> CreateOfferAsync(
        CreateAgencyOfferRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var profileId = AgencyProfileId.From(request.AgencyProfileId);
        var profileExists = await _db.AgencyProfiles.AnyAsync(x => x.Id == profileId, cancellationToken);
        if (!profileExists)
        {
            throw new KeyNotFoundException("AgencyProfile was not found.");
        }

        var terms = new AgencyOfferCommercialTerms(
            request.CommercialNotes,
            new AgencyOfferSalesRules(request.RequiresManualConfirmation, request.ExclusiveListing));
        var display = string.IsNullOrWhiteSpace(request.TitleOverride) && string.IsNullOrWhiteSpace(request.Highlight)
            ? AgencyOfferDisplaySettings.Empty()
            : new AgencyOfferDisplaySettings(request.TitleOverride, request.Highlight);

        var offer = AgencyOffer.Create(
            profileId,
            request.TourProductId,
            display,
            terms,
            ParseChannel(request.SalesChannel));
        ApplyDepartureScope(offer, request.DepartureScopeMode, request.DepartureScopeIds);
        _db.AgencyOffers.Add(offer);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(offer);
    }

    public async Task<AgencyOfferPanelResponse?> GetOfferAsync(
        Guid offerId,
        CancellationToken cancellationToken = default)
    {
        var offer = await _db.AgencyOffers.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == AgencyOfferId.From(offerId), cancellationToken);
        return offer is null ? null : Map(offer);
    }

    public async Task<IReadOnlyList<AgencyOfferPanelResponse>> ListOffersAsync(
        Guid agencyProfileId,
        CancellationToken cancellationToken = default)
    {
        var profileId = AgencyProfileId.From(agencyProfileId);
        var offers = await _db.AgencyOffers.AsNoTracking()
            .Where(x => x.AgencyProfileId == profileId)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);
        return offers.Select(Map).ToList();
    }

    public async Task ActivateOfferAsync(Guid offerId, CancellationToken cancellationToken = default)
    {
        var offer = await LoadOfferAsync(offerId, cancellationToken);
        offer.Activate();
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task ListOfferAsync(Guid offerId, CancellationToken cancellationToken = default)
    {
        var offer = await LoadOfferAsync(offerId, cancellationToken);
        offer.List();
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task OpenOfferSalesAsync(Guid offerId, CancellationToken cancellationToken = default)
    {
        var offer = await LoadOfferAsync(offerId, cancellationToken);
        offer.OpenSales();
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task CloseOfferSalesAsync(Guid offerId, CancellationToken cancellationToken = default)
    {
        var offer = await LoadOfferAsync(offerId, cancellationToken);
        offer.CloseSales();
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task SubmitOfferAsync(Guid offerId, CancellationToken cancellationToken = default)
    {
        var offer = await LoadOfferAsync(offerId, cancellationToken);
        offer.Submit();
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task ApproveOfferAsync(Guid offerId, CancellationToken cancellationToken = default)
    {
        var offer = await LoadOfferAsync(offerId, cancellationToken);
        offer.Approve();
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectOfferAsync(Guid offerId, CancellationToken cancellationToken = default)
    {
        var offer = await LoadOfferAsync(offerId, cancellationToken);
        offer.Reject();
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task PublishOfferAsync(Guid offerId, CancellationToken cancellationToken = default)
    {
        var offer = await LoadOfferAsync(offerId, cancellationToken);
        offer.Publish();
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UnpublishOfferAsync(Guid offerId, CancellationToken cancellationToken = default)
    {
        var offer = await LoadOfferAsync(offerId, cancellationToken);
        offer.Unpublish();
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task SuspendOfferAsync(Guid offerId, CancellationToken cancellationToken = default)
    {
        var offer = await LoadOfferAsync(offerId, cancellationToken);
        offer.Suspend();
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RetireOfferAsync(Guid offerId, CancellationToken cancellationToken = default)
    {
        var offer = await LoadOfferAsync(offerId, cancellationToken);
        offer.Retire();
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<AgencyOffer> LoadOfferAsync(Guid offerId, CancellationToken cancellationToken)
    {
        return await _db.AgencyOffers
                .SingleOrDefaultAsync(x => x.Id == AgencyOfferId.From(offerId), cancellationToken)
            ?? throw new KeyNotFoundException("AgencyOffer was not found.");
    }

    private static void ApplyDepartureScope(
        AgencyOffer offer,
        string? mode,
        IReadOnlyList<Guid>? departureIds)
    {
        if (string.Equals(mode, "Listed", StringComparison.OrdinalIgnoreCase))
        {
            offer.SetDepartureScopeListed(departureIds ?? Array.Empty<Guid>());
            return;
        }

        offer.SetDepartureScopeAll();
    }

    private static AgencyOfferSalesChannel ParseChannel(string? channel) =>
        Enum.TryParse<AgencyOfferSalesChannel>(channel, ignoreCase: true, out var parsed)
            ? parsed
            : AgencyOfferSalesChannel.Public;

    private static AgencyProfilePanelResponse Map(AgencyProfile profile) =>
        new(
            profile.Id.Value,
            profile.PartyId.Value,
            profile.Display.DisplayName,
            profile.Display.Description,
            profile.Display.LogoMediaAssetId,
            profile.Contact.PublicEmail,
            profile.Contact.PublicPhone,
            profile.Contact.WebsiteUrl,
            profile.Commercial.PublicListingEnabled,
            profile.Status.ToString());

    private static AgencyOfferPanelResponse Map(AgencyOffer offer) =>
        new(
            offer.Id.Value,
            offer.AgencyProfileId.Value,
            offer.TourProductId,
            offer.ReferencedTourDepartureId?.Value,
            offer.DepartureScopeMode.ToString(),
            offer.DepartureScopeIds.ToList(),
            offer.SalesChannel.ToString(),
            offer.Display.TitleOverride,
            offer.Display.Highlight,
            offer.CommercialTerms.Notes,
            offer.CommercialTerms.SalesRules.RequiresManualConfirmation,
            offer.CommercialTerms.SalesRules.ExclusiveListing,
            offer.SalesAvailability.SalesOpen,
            offer.Status.ToString(),
            offer.Visibility.ToString(),
            offer.PublicationStatus.ToString(),
            offer.CreatedAt.ToString(),
            offer.UpdatedAt.ToString());
}

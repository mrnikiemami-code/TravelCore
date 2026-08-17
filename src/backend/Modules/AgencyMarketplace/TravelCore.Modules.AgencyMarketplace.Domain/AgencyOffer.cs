namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Marketplace relationship between an AgencyProfile and a TourProduct (TC-P13-T003 / P13-R3).
/// Commercial terms are non-price metadata only (TC-P13-T004 / P13-R4).
/// </summary>
public sealed class AgencyOffer
{
    private AgencyOffer()
    {
        Display = null!;
        CommercialTerms = null!;
    }

    private AgencyOffer(
        AgencyOfferId id,
        AgencyProfileId agencyProfileId,
        Guid tourProductId,
        AgencyOfferDisplaySettings display,
        AgencyOfferCommercialTerms commercialTerms)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("AgencyOfferId cannot be empty.", nameof(id));
        }

        if (agencyProfileId.Value == Guid.Empty)
        {
            throw new ArgumentException("AgencyProfileId cannot be empty.", nameof(agencyProfileId));
        }

        if (tourProductId == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(tourProductId));
        }

        Id = id;
        AgencyProfileId = agencyProfileId;
        TourProductId = tourProductId;
        Display = display;
        CommercialTerms = commercialTerms;
        Status = AgencyOfferStatus.Draft;
        Visibility = AgencyOfferVisibility.Unlisted;
    }

    public AgencyOfferId Id { get; private set; }

    public AgencyProfileId AgencyProfileId { get; private set; }

    /// <summary>Logical TourProduct identity. No Tour schema FK.</summary>
    public Guid TourProductId { get; private set; }

    public AgencyOfferDisplaySettings Display { get; private set; }

    public AgencyOfferCommercialTerms CommercialTerms { get; private set; }

    public AgencyOfferStatus Status { get; private set; }

    public AgencyOfferVisibility Visibility { get; private set; }

    public static AgencyOffer Create(
        AgencyProfileId agencyProfileId,
        Guid tourProductId,
        AgencyOfferDisplaySettings? display = null,
        AgencyOfferCommercialTerms? commercialTerms = null)
    {
        return new AgencyOffer(
            AgencyOfferId.New(),
            agencyProfileId,
            tourProductId,
            display ?? AgencyOfferDisplaySettings.Empty(),
            commercialTerms ?? AgencyOfferCommercialTerms.Empty());
    }

    public void UpdateDisplay(AgencyOfferDisplaySettings display)
    {
        ArgumentNullException.ThrowIfNull(display);
        EnsureNotArchived();
        Display = display;
    }

    public void UpdateCommercialTerms(AgencyOfferCommercialTerms commercialTerms)
    {
        ArgumentNullException.ThrowIfNull(commercialTerms);
        EnsureNotArchived();
        CommercialTerms = commercialTerms;
    }

    public void Activate()
    {
        EnsureNotArchived();
        Status = AgencyOfferStatus.Active;
    }

    public void List()
    {
        EnsureNotArchived();
        if (Status != AgencyOfferStatus.Active)
        {
            throw new InvalidOperationException("Only an Active AgencyOffer can be Listed.");
        }

        Visibility = AgencyOfferVisibility.Listed;
    }

    public void Unlist()
    {
        EnsureNotArchived();
        Visibility = AgencyOfferVisibility.Unlisted;
    }

    /// <summary>
    /// Return an Active listing to Draft and unlist. Archived offers cannot be reopened here.
    /// </summary>
    public void Deactivate()
    {
        EnsureNotArchived();
        Visibility = AgencyOfferVisibility.Unlisted;
        Status = AgencyOfferStatus.Draft;
    }

    public void Archive()
    {
        Visibility = AgencyOfferVisibility.Unlisted;
        Status = AgencyOfferStatus.Archived;
    }

    private void EnsureNotArchived()
    {
        if (Status == AgencyOfferStatus.Archived)
        {
            throw new InvalidOperationException("Archived AgencyOffer cannot be changed.");
        }
    }
}

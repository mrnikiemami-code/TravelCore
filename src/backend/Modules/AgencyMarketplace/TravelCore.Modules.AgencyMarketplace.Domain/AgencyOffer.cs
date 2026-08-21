using NodaTime;

namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Marketplace relationship between an AgencyProfile and a TourProduct (TC-P13-T003 / P13-R3; P38-T003).
/// Commercial terms are non-price metadata only (TC-P13-T004 / P13-R4).
/// Sales availability is not capacity (TC-P13-T005 / P13-R5).
/// AgencyOffer ≠ TourDeparture ≠ Price ≠ Booking.
/// </summary>
public sealed class AgencyOffer
{
    private readonly List<Guid> _departureScopeIds = [];

    private AgencyOffer()
    {
        Display = null!;
        CommercialTerms = null!;
        SalesAvailability = null!;
    }

    private AgencyOffer(
        AgencyOfferId id,
        AgencyProfileId agencyProfileId,
        Guid tourProductId,
        AgencyOfferDisplaySettings display,
        AgencyOfferCommercialTerms commercialTerms,
        AgencyOfferSalesChannel salesChannel,
        Instant createdAt)
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
        SalesAvailability = AgencyOfferSalesAvailability.Closed();
        PublicationStatus = AgencyOfferPublicationStatus.Draft;
        Status = AgencyOfferStatus.Draft;
        Visibility = AgencyOfferVisibility.Unlisted;
        SalesChannel = salesChannel;
        DepartureScopeMode = AgencyOfferDepartureScopeMode.All;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public AgencyOfferId Id { get; private set; }

    public AgencyProfileId AgencyProfileId { get; private set; }

    /// <summary>Logical TourProduct identity. No Tour schema FK.</summary>
    public Guid TourProductId { get; private set; }

    public AgencyOfferDisplaySettings Display { get; private set; }

    public AgencyOfferCommercialTerms CommercialTerms { get; private set; }

    public AgencyOfferSalesAvailability SalesAvailability { get; private set; }

    /// <summary>Marketplace publication lifecycle. Not SEO IndexPolicy and not TourProduct catalog status.</summary>
    public AgencyOfferPublicationStatus PublicationStatus { get; private set; }

    /// <summary>Optional logical TourDeparture identity (compat). Prefer DepartureScope*.</summary>
    public MarketplaceTourDepartureId? ReferencedTourDepartureId { get; private set; }

    public AgencyOfferDepartureScopeMode DepartureScopeMode { get; private set; }

    /// <summary>Logical TourDeparture ids when mode is Listed. Empty when All.</summary>
    public IReadOnlyList<Guid> DepartureScopeIds => _departureScopeIds;

    public AgencyOfferSalesChannel SalesChannel { get; private set; }

    public AgencyOfferStatus Status { get; private set; }

    public AgencyOfferVisibility Visibility { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public static AgencyOffer Create(
        AgencyProfileId agencyProfileId,
        Guid tourProductId,
        AgencyOfferDisplaySettings? display = null,
        AgencyOfferCommercialTerms? commercialTerms = null,
        AgencyOfferSalesChannel salesChannel = AgencyOfferSalesChannel.Public,
        Instant? createdAt = null)
    {
        return new AgencyOffer(
            AgencyOfferId.New(),
            agencyProfileId,
            tourProductId,
            display ?? AgencyOfferDisplaySettings.Empty(),
            commercialTerms ?? AgencyOfferCommercialTerms.Empty(),
            salesChannel,
            createdAt ?? SystemClock.Instance.GetCurrentInstant());
    }

    public void UpdateDisplay(AgencyOfferDisplaySettings display)
    {
        ArgumentNullException.ThrowIfNull(display);
        EnsureMutable();
        Display = display;
        Touch();
    }

    public void UpdateCommercialTerms(AgencyOfferCommercialTerms commercialTerms)
    {
        ArgumentNullException.ThrowIfNull(commercialTerms);
        EnsureMutable();
        CommercialTerms = commercialTerms;
        Touch();
    }

    public void SetSalesChannel(AgencyOfferSalesChannel salesChannel)
    {
        EnsureMutable();
        SalesChannel = salesChannel;
        Touch();
    }

    public void SetDepartureScopeAll()
    {
        EnsureMutable();
        DepartureScopeMode = AgencyOfferDepartureScopeMode.All;
        _departureScopeIds.Clear();
        ReferencedTourDepartureId = null;
        Touch();
    }

    public void SetDepartureScopeListed(IEnumerable<Guid> departureIds)
    {
        ArgumentNullException.ThrowIfNull(departureIds);
        EnsureMutable();
        var list = departureIds.Distinct().ToList();
        if (list.Count == 0)
        {
            throw new ArgumentException("Listed departure scope requires at least one departure id.", nameof(departureIds));
        }

        if (list.Any(id => id == Guid.Empty))
        {
            throw new ArgumentException("Departure scope ids cannot be empty Guids.", nameof(departureIds));
        }

        DepartureScopeMode = AgencyOfferDepartureScopeMode.Listed;
        _departureScopeIds.Clear();
        _departureScopeIds.AddRange(list);
        ReferencedTourDepartureId = list.Count == 1
            ? MarketplaceTourDepartureId.From(list[0])
            : null;
        Touch();
    }

    public void Activate()
    {
        EnsureMutable();
        Status = AgencyOfferStatus.Active;
        Touch();
    }

    public void List()
    {
        EnsureMutable();
        if (Status != AgencyOfferStatus.Active)
        {
            throw new InvalidOperationException("Only an Active AgencyOffer can be Listed.");
        }

        Visibility = AgencyOfferVisibility.Listed;
        Touch();
    }

    public void OpenSales()
    {
        EnsureMutable();
        if (Status != AgencyOfferStatus.Active)
        {
            throw new InvalidOperationException("Only an Active AgencyOffer can open sales.");
        }

        SalesAvailability = AgencyOfferSalesAvailability.Open();
        Touch();
    }

    public void CloseSales()
    {
        EnsureMutable();
        SalesAvailability = AgencyOfferSalesAvailability.Closed();
        Touch();
    }

    public void SetReferencedTourDeparture(MarketplaceTourDepartureId? tourDepartureId)
    {
        EnsureMutable();
        ReferencedTourDepartureId = tourDepartureId;
        if (tourDepartureId is null)
        {
            DepartureScopeMode = AgencyOfferDepartureScopeMode.All;
            _departureScopeIds.Clear();
        }
        else
        {
            DepartureScopeMode = AgencyOfferDepartureScopeMode.Listed;
            _departureScopeIds.Clear();
            _departureScopeIds.Add(tourDepartureId.Value.Value);
        }

        Touch();
    }

    public void Submit()
    {
        EnsureMutable();
        if (PublicationStatus is not (AgencyOfferPublicationStatus.Draft or AgencyOfferPublicationStatus.Rejected))
        {
            throw new InvalidOperationException("Only Draft or Rejected AgencyOffer can be Submitted.");
        }

        PublicationStatus = AgencyOfferPublicationStatus.Submitted;
        Touch();
    }

    public void Approve()
    {
        EnsureMutable();
        if (PublicationStatus != AgencyOfferPublicationStatus.Submitted)
        {
            throw new InvalidOperationException("Only a Submitted AgencyOffer can be Approved.");
        }

        PublicationStatus = AgencyOfferPublicationStatus.Approved;
        Touch();
    }

    public void Reject()
    {
        EnsureMutable();
        if (PublicationStatus != AgencyOfferPublicationStatus.Submitted)
        {
            throw new InvalidOperationException("Only a Submitted AgencyOffer can be Rejected.");
        }

        PublicationStatus = AgencyOfferPublicationStatus.Rejected;
        Visibility = AgencyOfferVisibility.Unlisted;
        SalesAvailability = AgencyOfferSalesAvailability.Closed();
        Touch();
    }

    public void Publish()
    {
        EnsureMutable();
        if (PublicationStatus is not (AgencyOfferPublicationStatus.Approved or AgencyOfferPublicationStatus.Suspended))
        {
            throw new InvalidOperationException("Only an Approved or Suspended AgencyOffer can be Published.");
        }

        PublicationStatus = AgencyOfferPublicationStatus.Published;
        Visibility = AgencyOfferVisibility.Listed;
        Touch();
    }

    public void Unpublish()
    {
        EnsureMutable();
        if (PublicationStatus != AgencyOfferPublicationStatus.Published)
        {
            throw new InvalidOperationException("Only a Published AgencyOffer can be Unpublished.");
        }

        PublicationStatus = AgencyOfferPublicationStatus.Approved;
        Visibility = AgencyOfferVisibility.Unlisted;
        SalesAvailability = AgencyOfferSalesAvailability.Closed();
        Touch();
    }

    /// <summary>P38 Suspend — published offer temporarily off the public channel.</summary>
    public void Suspend()
    {
        EnsureMutable();
        if (PublicationStatus != AgencyOfferPublicationStatus.Published)
        {
            throw new InvalidOperationException("Only a Published AgencyOffer can be Suspended.");
        }

        PublicationStatus = AgencyOfferPublicationStatus.Suspended;
        Visibility = AgencyOfferVisibility.Unlisted;
        SalesAvailability = AgencyOfferSalesAvailability.Closed();
        Touch();
    }

    /// <summary>P38 Retire — terminal commercial retirement (distinct from Archived legacy path).</summary>
    public void Retire()
    {
        SalesAvailability = AgencyOfferSalesAvailability.Closed();
        Visibility = AgencyOfferVisibility.Unlisted;
        PublicationStatus = AgencyOfferPublicationStatus.Retired;
        Status = AgencyOfferStatus.Archived;
        Touch();
    }

    public void Unlist()
    {
        EnsureMutable();
        Visibility = AgencyOfferVisibility.Unlisted;
        Touch();
    }

    /// <summary>
    /// Return an Active listing to Draft and unlist. Archived/Retired offers cannot be reopened here.
    /// </summary>
    public void Deactivate()
    {
        EnsureMutable();
        SalesAvailability = AgencyOfferSalesAvailability.Closed();
        Visibility = AgencyOfferVisibility.Unlisted;
        if (PublicationStatus is AgencyOfferPublicationStatus.Published or AgencyOfferPublicationStatus.Suspended)
        {
            PublicationStatus = AgencyOfferPublicationStatus.Approved;
        }

        Status = AgencyOfferStatus.Draft;
        Touch();
    }

    public void Archive()
    {
        SalesAvailability = AgencyOfferSalesAvailability.Closed();
        Visibility = AgencyOfferVisibility.Unlisted;
        PublicationStatus = AgencyOfferPublicationStatus.Archived;
        Status = AgencyOfferStatus.Archived;
        Touch();
    }

    private void EnsureMutable()
    {
        if (Status == AgencyOfferStatus.Archived
            || PublicationStatus is AgencyOfferPublicationStatus.Archived or AgencyOfferPublicationStatus.Retired)
        {
            throw new InvalidOperationException("Archived or Retired AgencyOffer cannot be changed.");
        }
    }

    private void Touch()
    {
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();
    }
}

namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Marketplace commercial profile over Party identity (TC-P13-T002 / P13-R2).
/// 0..1 profile per Agency PartyId. Party remains identity SoR — this aggregate copies no Party table.
/// No Offer, Pricing, Commission, Booking, or Payment.
/// </summary>
public sealed class AgencyProfile
{
    private AgencyProfile()
    {
        Display = null!;
        Contact = null!;
        Commercial = null!;
    }

    private AgencyProfile(
        AgencyProfileId id,
        MarketplacePartyId partyId,
        AgencyDisplayInfo display,
        AgencyContactSettings contact,
        AgencyCommercialSettings commercial)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("AgencyProfileId cannot be empty.", nameof(id));
        }

        Id = id;
        PartyId = partyId;
        Display = display;
        Contact = contact;
        Commercial = commercial;
        Status = AgencyProfileStatus.Draft;
    }

    public AgencyProfileId Id { get; private set; }

    /// <summary>Logical Party identity. No Party schema FK.</summary>
    public MarketplacePartyId PartyId { get; private set; }

    public AgencyDisplayInfo Display { get; private set; }

    public AgencyContactSettings Contact { get; private set; }

    public AgencyCommercialSettings Commercial { get; private set; }

    public AgencyProfileStatus Status { get; private set; }

    public static AgencyProfile Create(
        MarketplacePartyId partyId,
        AgencyDisplayInfo display,
        AgencyContactSettings? contact = null,
        AgencyCommercialSettings? commercial = null)
    {
        ArgumentNullException.ThrowIfNull(display);
        return new AgencyProfile(
            AgencyProfileId.New(),
            partyId,
            display,
            contact ?? AgencyContactSettings.Empty(),
            commercial ?? AgencyCommercialSettings.Default());
    }

    public void UpdateDisplay(AgencyDisplayInfo display)
    {
        ArgumentNullException.ThrowIfNull(display);
        EnsureNotArchived();
        Display = display;
    }

    public void UpdateContact(AgencyContactSettings contact)
    {
        ArgumentNullException.ThrowIfNull(contact);
        EnsureNotArchived();
        Contact = contact;
    }

    public void UpdateCommercial(AgencyCommercialSettings commercial)
    {
        ArgumentNullException.ThrowIfNull(commercial);
        EnsureNotArchived();
        Commercial = commercial;
    }

    public void Activate()
    {
        EnsureNotArchived();
        Status = AgencyProfileStatus.Active;
    }

    public void Archive()
    {
        Status = AgencyProfileStatus.Archived;
    }

    private void EnsureNotArchived()
    {
        if (Status == AgencyProfileStatus.Archived)
        {
            throw new InvalidOperationException("Archived AgencyProfile cannot be changed.");
        }
    }
}

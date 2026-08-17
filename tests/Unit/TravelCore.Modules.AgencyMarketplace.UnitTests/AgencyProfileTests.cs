using TravelCore.Modules.AgencyMarketplace.Domain;
using Xunit;

namespace TravelCore.Modules.AgencyMarketplace.UnitTests;

/// <summary>
/// AgencyProfile commercial layer over Party identity (TC-P13-T002 / P13-R2).
/// </summary>
public sealed class AgencyProfileTests
{
    private static MarketplacePartyId Party() =>
        MarketplacePartyId.From(Guid.Parse("0198b3e0-0000-7000-8000-0000000000aa"));

    [Fact]
    public void Create_Starts_Draft_With_Display_And_Empty_Settings()
    {
        var profile = AgencyProfile.Create(
            Party(),
            new AgencyDisplayInfo("Istanbul Journeys", "Day tours", logoMediaAssetId: null));

        Assert.Equal(AgencyProfileStatus.Draft, profile.Status);
        Assert.Equal(Party(), profile.PartyId);
        Assert.Equal("Istanbul Journeys", profile.Display.DisplayName);
        Assert.Equal("Day tours", profile.Display.Description);
        Assert.Null(profile.Display.LogoMediaAssetId);
        Assert.Null(profile.Contact.PublicEmail);
        Assert.False(profile.Commercial.PublicListingEnabled);
    }

    [Fact]
    public void Activate_And_Archive_Lifecycle()
    {
        var profile = AgencyProfile.Create(Party(), new AgencyDisplayInfo("A", null, null));
        profile.Activate();
        Assert.Equal(AgencyProfileStatus.Active, profile.Status);

        profile.Archive();
        Assert.Equal(AgencyProfileStatus.Archived, profile.Status);
        Assert.Throws<InvalidOperationException>(() =>
            profile.UpdateDisplay(new AgencyDisplayInfo("B", null, null)));
        Assert.Throws<InvalidOperationException>(profile.Activate);
    }

    [Fact]
    public void Create_Rejects_Empty_DisplayName_And_Empty_Logo()
    {
        Assert.Throws<ArgumentException>(() =>
            AgencyProfile.Create(Party(), new AgencyDisplayInfo("  ", null, null)));
        Assert.Throws<ArgumentException>(() =>
            new AgencyDisplayInfo("Ok", null, Guid.Empty));
    }

    [Fact]
    public void UpdateContact_And_Commercial_Replace_Owned_Values()
    {
        var profile = AgencyProfile.Create(Party(), new AgencyDisplayInfo("A", null, null));
        var logo = Guid.Parse("0198b3e0-0000-7000-8000-0000000000bb");

        profile.UpdateDisplay(new AgencyDisplayInfo("B", "desc", logo));
        profile.UpdateContact(new AgencyContactSettings("sales@example.com", "+90", "https://example.com"));
        profile.UpdateCommercial(new AgencyCommercialSettings(publicListingEnabled: true));

        Assert.Equal("B", profile.Display.DisplayName);
        Assert.Equal(logo, profile.Display.LogoMediaAssetId);
        Assert.Equal("sales@example.com", profile.Contact.PublicEmail);
        Assert.True(profile.Commercial.PublicListingEnabled);
    }
}

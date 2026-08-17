using TravelCore.Modules.AgencyMarketplace.Domain;
using Xunit;

namespace TravelCore.Modules.AgencyMarketplace.UnitTests;

/// <summary>
/// AgencyOffer marketplace listing over TourProduct (TC-P13-T003 / P13-R3).
/// </summary>
public sealed class AgencyOfferTests
{
    private static AgencyProfileId Profile() =>
        AgencyProfileId.From(Guid.Parse("0198b3e0-0000-7000-8000-0000000000cc"));

    private static Guid Tour() => Guid.Parse("0198b3e0-0000-7000-8000-0000000000dd");

    [Fact]
    public void Create_Starts_Draft_Unlisted()
    {
        var offer = AgencyOffer.Create(Profile(), Tour());
        Assert.Equal(AgencyOfferStatus.Draft, offer.Status);
        Assert.Equal(AgencyOfferVisibility.Unlisted, offer.Visibility);
        Assert.Equal(Tour(), offer.TourProductId);
        Assert.Null(offer.Display.TitleOverride);
        Assert.Null(offer.CommercialTerms.Notes);
    }

    [Fact]
    public void List_Requires_Active_Then_Archive_Unlists()
    {
        var offer = AgencyOffer.Create(Profile(), Tour());
        Assert.Throws<InvalidOperationException>(offer.List);

        offer.Activate();
        offer.List();
        Assert.Equal(AgencyOfferVisibility.Listed, offer.Visibility);

        offer.Archive();
        Assert.Equal(AgencyOfferStatus.Archived, offer.Status);
        Assert.Equal(AgencyOfferVisibility.Unlisted, offer.Visibility);
        Assert.Throws<InvalidOperationException>(offer.Activate);
    }

    [Fact]
    public void Create_Rejects_Empty_TourProductId()
    {
        Assert.Throws<ArgumentException>(() => AgencyOffer.Create(Profile(), Guid.Empty));
    }
}

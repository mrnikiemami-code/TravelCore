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
        Assert.False(offer.CommercialTerms.SalesRules.RequiresManualConfirmation);
        Assert.False(offer.CommercialTerms.SalesRules.ExclusiveListing);
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
    public void CommercialTerms_Carry_SalesRules_Without_Money()
    {
        var terms = new AgencyOfferCommercialTerms(
            "partner desk",
            new AgencyOfferSalesRules(requiresManualConfirmation: true, exclusiveListing: true));
        var offer = AgencyOffer.Create(Profile(), Tour(), commercialTerms: terms);

        Assert.Equal("partner desk", offer.CommercialTerms.Notes);
        Assert.True(offer.CommercialTerms.SalesRules.RequiresManualConfirmation);
        Assert.True(offer.CommercialTerms.SalesRules.ExclusiveListing);
        Assert.Null(typeof(AgencyOfferCommercialTerms).GetProperty("PriceOverride"));
        Assert.Null(typeof(AgencyOfferCommercialTerms).GetProperty("Discount"));
        Assert.Null(typeof(AgencyOfferCommercialTerms).GetProperty("Commission"));
        Assert.Null(typeof(AgencyOffer).GetProperty("Amount"));
        Assert.Null(typeof(AgencyOffer).GetProperty("Currency"));
    }

    [Fact]
    public void Deactivate_Unlists_And_Returns_To_Draft()
    {
        var offer = AgencyOffer.Create(Profile(), Tour());
        offer.Activate();
        offer.List();
        offer.Deactivate();
        Assert.Equal(AgencyOfferStatus.Draft, offer.Status);
        Assert.Equal(AgencyOfferVisibility.Unlisted, offer.Visibility);

        offer.Archive();
        Assert.Throws<InvalidOperationException>(offer.Deactivate);
    }

    [Fact]
    public void OpenSales_Requires_Active_And_Does_Not_Own_Capacity()
    {
        var offer = AgencyOffer.Create(Profile(), Tour());
        Assert.False(offer.SalesAvailability.SalesOpen);
        Assert.Null(offer.ReferencedTourDepartureId);
        Assert.Throws<InvalidOperationException>(offer.OpenSales);

        offer.Activate();
        offer.OpenSales();
        Assert.True(offer.SalesAvailability.SalesOpen);

        var departure = MarketplaceTourDepartureId.From(Guid.Parse("0198b3e0-0000-7000-8000-0000000000ee"));
        offer.SetReferencedTourDeparture(departure);
        Assert.Equal(departure, offer.ReferencedTourDepartureId);

        offer.Deactivate();
        Assert.False(offer.SalesAvailability.SalesOpen);
        Assert.Null(typeof(AgencyOffer).GetProperty("AvailableSeats"));
        Assert.Null(typeof(AgencyOffer).GetProperty("ReservedSeats"));
        Assert.Null(typeof(AgencyOffer).GetProperty("Capacity"));
    }

    [Fact]
    public void Create_Rejects_Empty_TourProductId()
    {
        Assert.Throws<ArgumentException>(() => AgencyOffer.Create(Profile(), Guid.Empty));
    }
}

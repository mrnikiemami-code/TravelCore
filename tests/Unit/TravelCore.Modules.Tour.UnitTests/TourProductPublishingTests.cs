using NodaTime;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.Modules.Tour.UnitTests;

public sealed class TourProductPublishingTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 7, 0);

    [Fact]
    public void NewTourProduct_DefaultsToDraft()
    {
        var product = TourProduct.CreateExperience("EXP-PUB-001", "Name", Now);
        Assert.Equal(TourCatalogStatus.Draft, product.CatalogStatus);
    }

    [Fact]
    public void SetCatalogStatus_AcceptsClosedSet()
    {
        var product = TourProduct.CreatePackage("PKG-PUB-001", "Name", Now);
        product.SetCatalogStatus(TourCatalogStatus.Published, Now);
        Assert.Equal(TourCatalogStatus.Published, product.CatalogStatus);

        product.SetCatalogStatus(TourCatalogStatus.Inactive, Now.Plus(Duration.FromMinutes(1)));
        Assert.Equal(TourCatalogStatus.Inactive, product.CatalogStatus);
    }

    [Fact]
    public void SetTranslationSlug_NormalizesAndClears()
    {
        var product = TourProduct.CreateExperience("EXP-SLUG-001", "Name", Now);
        product.UpsertTranslation("fa", "عنوان", null, Now);

        product.SetTranslationSlug("FA", " caspian-walk ", Now.Plus(Duration.FromMinutes(1)));
        Assert.Equal("caspian-walk", product.FindTranslation("fa")!.Slug);

        product.SetTranslationSlug("fa", "  ", Now.Plus(Duration.FromMinutes(2)));
        Assert.Null(product.FindTranslation("fa")!.Slug);
    }

    [Fact]
    public void SetTranslationSlug_RequiresExistingTranslation()
    {
        var product = TourProduct.CreateExperience("EXP-SLUG-002", "Name", Now);
        Assert.Throws<InvalidOperationException>(() =>
            product.SetTranslationSlug("en", "missing-row", Now));
    }

    [Fact]
    public void NormalizeSlug_RejectsInvalidShapes()
    {
        Assert.ThrowsAny<ArgumentException>(() => TourProductTranslation.NormalizeSlug("-bad"));
        Assert.ThrowsAny<ArgumentException>(() => TourProductTranslation.NormalizeSlug("bad--slug"));
        Assert.ThrowsAny<ArgumentException>(() => TourProductTranslation.NormalizeSlug("Bad Slug"));
    }
}

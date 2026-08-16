using TravelCore.Modules.Seo.Domain;
using Xunit;

namespace TravelCore.Modules.Seo.UnitTests;

public sealed class SeoStructuredDataEngineTests
{
    [Fact]
    public void BuildBreadcrumbList_EmitsTruthfulNodes_OmitsBlank()
    {
        var doc = SeoStructuredDataEngine.BuildBreadcrumbList(
            "en",
            [
                new SeoBreadcrumbNodeInput("Turkey", "destinations/turkey"),
                new SeoBreadcrumbNodeInput("Istanbul", "destinations/istanbul"),
                new SeoBreadcrumbNodeInput("  ", null),
            ]);

        Assert.NotNull(doc);
        Assert.Equal("BreadcrumbList", doc.Type);
        Assert.Equal(2, doc.ItemListElement.Count);
        Assert.Equal(1, doc.ItemListElement[0].Position);
        Assert.Equal("Turkey", doc.ItemListElement[0].Name);
        Assert.Equal("/en/destinations/turkey", doc.ItemListElement[0].Item);
        Assert.Equal("/en/destinations/istanbul", doc.ItemListElement[1].Item);
    }

    [Fact]
    public void BuildBreadcrumbList_OmitsItemHref_WhenPathMissing()
    {
        var doc = SeoStructuredDataEngine.BuildBreadcrumbList(
            "fa",
            [new SeoBreadcrumbNodeInput("بدون اسلاگ", null)]);

        Assert.NotNull(doc);
        Assert.Null(doc.ItemListElement[0].Item);
        Assert.Equal("بدون اسلاگ", doc.ItemListElement[0].Name);
    }

    [Fact]
    public void BuildBreadcrumbList_ReturnsNull_WhenNoValidNodes()
    {
        var doc = SeoStructuredDataEngine.BuildBreadcrumbList(
            "en",
            [new SeoBreadcrumbNodeInput(" ", null)]);

        Assert.Null(doc);
    }
}

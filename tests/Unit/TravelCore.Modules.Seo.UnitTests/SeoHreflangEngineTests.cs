using NodaTime;
using TravelCore.Modules.Seo.Domain;
using Xunit;

namespace TravelCore.Modules.Seo.UnitTests;

public sealed class SeoHreflangEngineTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 16, 10, 0);
    private static readonly Guid ResourceId = Guid.Parse("0198a000-0000-7000-8000-000000000501");

    [Fact]
    public void BuildAlternates_EmitsFaEnPair_WhenBothRoutesExist()
    {
        var en = SeoRoute.Create(SeoResourceType.Destination, ResourceId, "en", "destinations/istanbul", Now);
        var fa = SeoRoute.Create(SeoResourceType.Destination, ResourceId, "fa", "destinations/استانبول", Now);

        var alternates = SeoHreflangEngine.BuildAlternates(SeoResourceType.Destination, ResourceId, [en, fa]);

        Assert.Equal(2, alternates.Count);
        Assert.Contains(alternates, a => a.Locale == "en" && a.Href == "/en/destinations/istanbul");
        Assert.Contains(alternates, a => a.Locale == "fa" && a.Href == "/fa/destinations/استانبول");
    }

    [Fact]
    public void BuildAlternates_OmitsMissingLocale_DoesNotFabricate()
    {
        var en = SeoRoute.Create(SeoResourceType.Destination, ResourceId, "en", "destinations/istanbul", Now);

        var alternates = SeoHreflangEngine.BuildAlternates(SeoResourceType.Destination, ResourceId, [en]);

        Assert.Single(alternates);
        Assert.Equal("en", alternates[0].Locale);
        Assert.DoesNotContain(alternates, a => a.Locale == "fa");
    }

    [Fact]
    public void BuildAlternatesOmittingMissing_SkipsRequestedButUnavailableLocales()
    {
        var en = SeoRoute.Create(SeoResourceType.Destination, ResourceId, "en", "destinations/istanbul", Now);

        var alternates = SeoHreflangEngine.BuildAlternatesOmittingMissing(
            SeoResourceType.Destination,
            ResourceId,
            [en],
            ["fa", "en", "ar"]);

        Assert.Single(alternates);
        Assert.Equal("en", alternates[0].Locale);
    }

    [Fact]
    public void BuildAlternates_IgnoresOtherResources()
    {
        var other = Guid.Parse("0198a000-0000-7000-8000-000000000502");
        var en = SeoRoute.Create(SeoResourceType.Destination, ResourceId, "en", "destinations/istanbul", Now);
        var foreign = SeoRoute.Create(SeoResourceType.Destination, other, "fa", "destinations/other", Now);

        var alternates = SeoHreflangEngine.BuildAlternates(SeoResourceType.Destination, ResourceId, [en, foreign]);

        Assert.Single(alternates);
        Assert.Equal("en", alternates[0].Locale);
    }
}

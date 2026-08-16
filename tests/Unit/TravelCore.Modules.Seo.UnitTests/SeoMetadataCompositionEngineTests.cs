using TravelCore.Modules.Seo.Domain;
using Xunit;

namespace TravelCore.Modules.Seo.UnitTests;

public sealed class SeoMetadataCompositionEngineTests
{
    [Fact]
    public void Compose_UsesContent_WhenNoOverride()
    {
        var composed = SeoMetadataCompositionEngine.Compose(
            new SeoMetadataContentInput("Istanbul", "City on two continents"));

        Assert.Equal("Istanbul", composed.Title);
        Assert.Equal("City on two continents", composed.Description);
        Assert.False(composed.UsedTitleOverride);
        Assert.False(composed.UsedDescriptionOverride);
    }

    [Fact]
    public void Compose_OverrideWins_DoesNotCopyCmsOwnership()
    {
        var composed = SeoMetadataCompositionEngine.Compose(
            new SeoMetadataContentInput("Istanbul", "Domain description"),
            new SeoMetadataOverrideValues("Istanbul Travel Guide", "SEO description override"));

        Assert.Equal("Istanbul Travel Guide", composed.Title);
        Assert.Equal("SEO description override", composed.Description);
        Assert.True(composed.UsedTitleOverride);
        Assert.True(composed.UsedDescriptionOverride);
    }

    [Fact]
    public void Compose_EmptyContentTitle_FallsBackToBrand()
    {
        var composed = SeoMetadataCompositionEngine.Compose(
            new SeoMetadataContentInput("  ", null));

        Assert.Equal(SeoMetadataCompositionEngine.DefaultBrandTitle, composed.Title);
        Assert.Null(composed.Description);
    }

    [Fact]
    public void Compose_PartialOverride_KeepsContentDescription()
    {
        var composed = SeoMetadataCompositionEngine.Compose(
            new SeoMetadataContentInput("Istanbul", "Domain description"),
            new SeoMetadataOverrideValues("Override title", null));

        Assert.Equal("Override title", composed.Title);
        Assert.Equal("Domain description", composed.Description);
        Assert.True(composed.UsedTitleOverride);
        Assert.False(composed.UsedDescriptionOverride);
    }
}

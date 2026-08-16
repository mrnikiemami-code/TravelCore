using NodaTime;
using TravelCore.Modules.Content.Domain;
using Xunit;
using ContentItemAggregate = TravelCore.Modules.Content.Domain.ContentItem;

namespace TravelCore.Modules.Content.UnitTests;

public sealed class ContentItemTranslationTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 16, 21, 0);

    [Fact]
    public void UpsertTranslation_CreatesLocaleRow_WithoutSlug()
    {
        var item = ContentItemAggregate.CreateArticle("ART-LOC-1", "Demo", Now);
        var translation = item.UpsertTranslation(
            "FA",
            "  عنوان فارسی  ",
            "متن بدنه",
            "خلاصه",
            Now);

        Assert.Equal("fa", translation.LocaleCode);
        Assert.Equal("عنوان فارسی", translation.Title);
        Assert.Equal("متن بدنه", translation.Body);
        Assert.Equal("خلاصه", translation.Excerpt);
        Assert.Null(translation.Slug);
        Assert.Single(item.Translations);
        Assert.Equal(item.Id, translation.ContentItemId);
    }

    [Fact]
    public void SetTranslationSlug_RequiresExistingTranslation_AndNormalizes()
    {
        var item = ContentItemAggregate.CreateArticle("ART-LOC-SLUG", "Demo", Now);
        item.UpsertTranslation("en", "Title", null, null, Now);
        var translation = item.SetTranslationSlug("EN", "  Summer-Tips  ", Now.Plus(Duration.FromMinutes(1)));

        Assert.Equal("summer-tips", translation.Slug);
        Assert.Equal("en", translation.LocaleCode);
    }

    [Fact]
    public void SetTranslationSlug_RejectsMissingTranslation()
    {
        var item = ContentItemAggregate.CreateArticle("ART-LOC-MISS", "Demo", Now);
        Assert.ThrowsAny<ArgumentException>(() =>
            item.SetTranslationSlug("en", "missing-row", Now));
    }

    [Fact]
    public void NormalizeSlug_RejectsInvalidShapes()
    {
        Assert.Null(ContentItemTranslation.NormalizeSlug("  "));
        Assert.Equal("ok-slug", ContentItemTranslation.NormalizeSlug("Ok-Slug"));
        Assert.ThrowsAny<ArgumentException>(() => ContentItemTranslation.NormalizeSlug("-bad"));
        Assert.ThrowsAny<ArgumentException>(() => ContentItemTranslation.NormalizeSlug("bad--slug"));
        Assert.ThrowsAny<ArgumentException>(() => ContentItemTranslation.NormalizeSlug("bad_slug"));
    }

    [Fact]
    public void UpsertTranslation_UpdatesExistingLocale()
    {
        var item = ContentItemAggregate.CreateArticle("ART-LOC-2", "Demo", Now);
        item.UpsertTranslation("en", "Title One", null, null, Now);
        var updated = item.UpsertTranslation(
            "en",
            "Title Two",
            "Body Two",
            "Excerpt Two",
            Now.Plus(Duration.FromMinutes(1)));

        Assert.Single(item.Translations);
        Assert.Equal("Title Two", updated.Title);
        Assert.Equal("Body Two", updated.Body);
        Assert.Equal("Excerpt Two", updated.Excerpt);
    }

    [Fact]
    public void FindTranslation_IsExactLocaleOnly()
    {
        var item = ContentItemAggregate.CreateLandingPage("LND-LOC-1", "Landing", Now);
        item.UpsertTranslation("fa", "عنوان", null, null, Now);

        Assert.NotNull(item.FindTranslation("FA"));
        Assert.Null(item.FindTranslation("en"));
    }

    [Fact]
    public void UpsertTranslation_RejectsBlankTitle()
    {
        var item = ContentItemAggregate.CreateGuide("GDE-LOC-1", "Guide", Now);
        Assert.ThrowsAny<ArgumentException>(() =>
            item.UpsertTranslation("en", "  ", null, null, Now));
    }

    [Fact]
    public void NormalizeLocaleCode_PreservesBcp47Shape()
    {
        Assert.Equal("en-US", ContentItemTranslation.NormalizeLocaleCode("en-us"));
        Assert.Equal("fa", ContentItemTranslation.NormalizeLocaleCode(" FA "));
    }
}

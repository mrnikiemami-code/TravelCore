using NodaTime;
using TravelCore.Modules.Content.Domain;
using Xunit;
using ContentItemAggregate = TravelCore.Modules.Content.Domain.ContentItem;

namespace TravelCore.Modules.Content.UnitTests;

public sealed class ContentItemTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 16, 20, 0);

    [Fact]
    public void ContentItemId_RejectsEmpty()
    {
        Assert.Throws<ArgumentException>(() => ContentItemId.From(Guid.Empty));
    }

    [Fact]
    public void ContentItemId_New_IsNonEmpty()
    {
        var id = ContentItemId.New();
        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void CreateArticle_SetsKindAndSpecialization()
    {
        var item = ContentItemAggregate.CreateArticle("ART-001", " Grand Guide ", Now);

        Assert.Equal(ContentKind.Article, item.Kind);
        Assert.Equal("ART-001", item.Code);
        Assert.Equal("Grand Guide", item.EnglishName);
        Assert.NotNull(item.Article);
        Assert.Null(item.LandingPage);
        Assert.Null(item.Guide);
        Assert.Equal(item.Id, item.Article.ContentItemId);
        Assert.NotEqual(Guid.Empty, item.Id.Value);
    }

    [Fact]
    public void CreateLandingPage_SetsKindAndSpecialization()
    {
        var item = ContentItemAggregate.CreateLandingPage("LND-001", "Landing", Now);

        Assert.Equal(ContentKind.LandingPage, item.Kind);
        Assert.NotNull(item.LandingPage);
        Assert.Null(item.Article);
        Assert.Null(item.Guide);
    }

    [Fact]
    public void CreateGuide_SetsKindAndSpecialization()
    {
        var item = ContentItemAggregate.CreateGuide("GDE-001", "City Guide", Now);

        Assert.Equal(ContentKind.Guide, item.Kind);
        Assert.NotNull(item.Guide);
        Assert.Null(item.Article);
        Assert.Null(item.LandingPage);
    }

    [Fact]
    public void ValidateSpecializationInvariant_RejectsMultiKind()
    {
        var id = ContentItemId.New();
        var article = Article.Create(id);
        var guide = Guide.Create(id);

        var ex = Assert.Throws<ArgumentException>(() =>
            ContentItemAggregate.ValidateSpecializationInvariant(
                id,
                ContentKind.Article,
                article,
                landingPage: null,
                guide));

        Assert.Contains("only one typed specialization", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateSpecializationInvariant_RejectsKindMismatch()
    {
        var id = ContentItemId.New();
        var guide = Guide.Create(id);

        Assert.Throws<ArgumentException>(() =>
            ContentItemAggregate.ValidateSpecializationInvariant(
                id,
                ContentKind.Article,
                article: null,
                landingPage: null,
                guide));
    }

    [Fact]
    public void CreateArticle_RejectsBlankCode()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            ContentItemAggregate.CreateArticle("  ", "Name", Now));
    }
}

using NodaTime;
using TravelCore.Modules.Content.Domain;
using Xunit;
using ContentItemAggregate = TravelCore.Modules.Content.Domain.ContentItem;

namespace TravelCore.Modules.Content.UnitTests;

public sealed class ContentTaxonomyTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 16, 22, 0);

    [Fact]
    public void ContentCategory_Create_NormalizesFields()
    {
        var category = ContentCategory.Create("  travel-tips ", " Travel Tips ", Now);
        Assert.Equal("travel-tips", category.Code);
        Assert.Equal("Travel Tips", category.EnglishName);
        Assert.NotEqual(Guid.Empty, category.Id.Value);
    }

    [Fact]
    public void ContentTag_Create_NormalizesFields()
    {
        var tag = ContentTag.Create("visa", "Visa", Now);
        Assert.Equal("visa", tag.Code);
        Assert.Equal("Visa", tag.EnglishName);
    }

    [Fact]
    public void AssignCategoryAndTag_AreIdempotentAndBounded()
    {
        var item = ContentItemAggregate.CreateArticle("ART-TAX-1", "Taxonomy Demo", Now);
        var categoryId = ContentCategoryId.New();
        var tagId = ContentTagId.New();

        item.AssignCategory(categoryId, Now);
        item.AssignCategory(categoryId, Now);
        item.AssignTag(tagId, Now);
        item.AssignTag(tagId, Now);

        Assert.Single(item.Categories);
        Assert.Single(item.Tags);
        Assert.True(item.RemoveCategory(categoryId, Now));
        Assert.True(item.RemoveTag(tagId, Now));
        Assert.Empty(item.Categories);
        Assert.Empty(item.Tags);
    }

    [Fact]
    public void ContentCategoryId_RejectsEmpty()
    {
        Assert.Throws<ArgumentException>(() => ContentCategoryId.From(Guid.Empty));
    }
}

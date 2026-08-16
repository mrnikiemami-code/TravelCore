using NodaTime;
using TravelCore.Modules.Content.Domain;
using Xunit;
using ContentItemAggregate = TravelCore.Modules.Content.Domain.ContentItem;

namespace TravelCore.Modules.Content.UnitTests;

public sealed class ContentBlockTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 16, 23, 0);

    [Fact]
    public void AddHeadingAndParagraph_OrdersRelationally()
    {
        var item = ContentItemAggregate.CreateArticle("ART-BLK-1", "Blocks", Now);
        var h = item.AddHeadingBlock("Title", 2, Now);
        var p = item.AddParagraphBlock("Body text", Now);

        Assert.Equal(ContentBlockKind.Heading, h.Kind);
        Assert.Equal((short)2, h.HeadingLevel);
        Assert.Equal(0, h.SortOrder);
        Assert.Equal(1, p.SortOrder);
        Assert.Equal(2, item.Blocks.Count);
    }

    [Fact]
    public void AddGalleryAndFaq_PersistChildRows()
    {
        var item = ContentItemAggregate.CreateLandingPage("LND-BLK-1", "Landing", Now);
        var m1 = Guid.NewGuid();
        var m2 = Guid.NewGuid();
        var gallery = item.AddGalleryBlock([m1, m2], Now);
        var faq = item.AddFaqBlock([("Q1", "A1"), ("Q2", "A2")], Now);

        Assert.Equal(2, gallery.GalleryItems.Count);
        Assert.Equal(2, faq.FaqItems.Count);
        Assert.Equal(ContentBlockKind.Gallery, gallery.Kind);
        Assert.Equal(ContentBlockKind.Faq, faq.Kind);
    }

    [Fact]
    public void ReorderBlocks_RewritesSortOrder()
    {
        var item = ContentItemAggregate.CreateGuide("GDE-BLK-1", "Guide", Now);
        var a = item.AddParagraphBlock("A", Now);
        var b = item.AddParagraphBlock("B", Now);
        var ordered = item.ReorderBlocks([b.Id, a.Id], Now);

        Assert.Equal(b.Id, ordered[0].Id);
        Assert.Equal(0, ordered[0].SortOrder);
        Assert.Equal(a.Id, ordered[1].Id);
        Assert.Equal(1, ordered[1].SortOrder);
    }

    [Fact]
    public void ImageRequiresMediaAssetId_WidgetsNotPresent()
    {
        var item = ContentItemAggregate.CreateArticle("ART-BLK-2", "Blocks", Now);
        Assert.ThrowsAny<ArgumentException>(() =>
            item.AddImageBlock(Guid.Empty, Now));
        Assert.False(Enum.IsDefined(typeof(ContentBlockKind), (short)99));
        Assert.Equal(8, Enum.GetValues<ContentBlockKind>().Length);
    }
}

using NodaTime;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.Modules.Tour.UnitTests;

public sealed class TourProductMediaLinkTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 6, 0);

    [Fact]
    public void SetCover_ReplacesAndIsIdempotent()
    {
        var product = TourProduct.CreateExperience("EXP-MED-001", "Name", Now);
        var a = Guid.Parse("01900000-0000-7000-8000-000000000401");
        var b = Guid.Parse("01900000-0000-7000-8000-000000000402");

        product.SetCover(a, Now);
        Assert.Equal(a, product.Cover!.MediaAssetId);

        product.SetCover(a, Now.Plus(Duration.FromSeconds(1)));
        Assert.Single(product.MediaLinks);

        product.SetCover(b, Now.Plus(Duration.FromSeconds(2)));
        Assert.Equal(b, product.Cover!.MediaAssetId);
        Assert.Single(product.MediaLinks);
    }

    [Fact]
    public void CoverAndGallery_AreMutuallyExclusivePerAsset()
    {
        var product = TourProduct.CreatePackage("PKG-MED-001", "Name", Now);
        var id = Guid.Parse("01900000-0000-7000-8000-000000000403");
        product.AddGalleryItem(id, Now);

        Assert.Throws<InvalidOperationException>(() => product.SetCover(id, Now));
    }

    [Fact]
    public void ReorderGallery_NormalizesSortOrder()
    {
        var product = TourProduct.CreateExperience("EXP-MED-002", "Name", Now);
        var a = Guid.Parse("01900000-0000-7000-8000-000000000411");
        var b = Guid.Parse("01900000-0000-7000-8000-000000000412");
        var c = Guid.Parse("01900000-0000-7000-8000-000000000413");
        product.AddGalleryItem(a, Now);
        product.AddGalleryItem(b, Now);
        product.AddGalleryItem(c, Now);

        var ordered = product.ReorderGallery([c, a, b], Now.Plus(Duration.FromMinutes(1)));
        Assert.Equal([c, a, b], ordered.Select(x => x.MediaAssetId).ToArray());
        Assert.Equal([0, 1, 2], ordered.Select(x => x.SortOrder).ToArray());
    }

    [Fact]
    public void RemoveCover_ClearsOnlyCover()
    {
        var product = TourProduct.CreatePackage("PKG-MED-002", "Name", Now);
        var cover = Guid.Parse("01900000-0000-7000-8000-000000000421");
        var gallery = Guid.Parse("01900000-0000-7000-8000-000000000422");
        product.SetCover(cover, Now);
        product.AddGalleryItem(gallery, Now);

        product.RemoveCover(Now.Plus(Duration.FromMinutes(1)));
        Assert.Null(product.Cover);
        Assert.Single(product.GalleryOrdered);
    }
}

using NodaTime;
using TravelCore.Modules.Place.Domain;
using Xunit;
using PlaceAggregate = TravelCore.Modules.Place.Domain.Place;

namespace TravelCore.Modules.Place.UnitTests;

public sealed class PlaceMediaLinkTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 16, 21, 30);

    [Fact]
    public void SetCover_ReplacesExistingCover_AndRejectsEmpty()
    {
        var place = PlaceAggregate.CreateHotel("HTL-MED-1", "Media Hotel", Now, starRating: 4);
        var first = Guid.Parse("01900000-0000-7000-8000-000000000101");
        var second = Guid.Parse("01900000-0000-7000-8000-000000000102");

        place.SetCover(first, Now);
        Assert.Equal(first, place.Cover!.MediaAssetId);
        Assert.Equal(0, place.Cover.SortOrder);
        Assert.Equal(PlaceMediaRole.Cover, place.Cover.Role);

        place.SetCover(second, Now);
        Assert.Equal(second, place.Cover!.MediaAssetId);
        Assert.Single(place.MediaLinks.Where(x => x.Role == PlaceMediaRole.Cover));

        Assert.Throws<ArgumentException>(() => place.SetCover(Guid.Empty, Now));
    }

    [Fact]
    public void SameAsset_CannotBeCoverAndGallery()
    {
        var place = PlaceAggregate.CreateHotel("HTL-MED-2", "Dup Hotel", Now, starRating: 3);
        var assetId = Guid.Parse("01900000-0000-7000-8000-000000000201");

        place.AddGalleryItem(assetId, Now, sortOrder: 10);
        Assert.Throws<InvalidOperationException>(() => place.SetCover(assetId, Now));

        place.RemoveGalleryItem(assetId, Now);
        place.SetCover(assetId, Now);
        Assert.Throws<InvalidOperationException>(() => place.AddGalleryItem(assetId, Now));
    }

    [Fact]
    public void Gallery_SortOrderUnique_ContiguityNotRequired_ReorderNormalizes()
    {
        var place = PlaceAggregate.CreateAttraction("ATR-MED-1", "Gallery POI", Now);
        var a = Guid.Parse("01900000-0000-7000-8000-000000000301");
        var b = Guid.Parse("01900000-0000-7000-8000-000000000302");
        var c = Guid.Parse("01900000-0000-7000-8000-000000000303");

        place.AddGalleryItem(a, Now, sortOrder: 0);
        place.AddGalleryItem(b, Now, sortOrder: 10);
        place.AddGalleryItem(c, Now, sortOrder: 20);
        Assert.Equal([0, 10, 20], place.GalleryOrdered.Select(x => x.SortOrder).ToArray());

        Assert.Throws<ArgumentException>(() => place.AddGalleryItem(
            Guid.Parse("01900000-0000-7000-8000-000000000399"),
            Now,
            sortOrder: 10));

        place.ReorderGallery([c, a, b], Now);
        Assert.Equal([c, a, b], place.GalleryOrdered.Select(x => x.MediaAssetId).ToArray());
        Assert.Equal([0, 1, 2], place.GalleryOrdered.Select(x => x.SortOrder).ToArray());
    }

    [Fact]
    public void RemoveCoverAndGallery_DoesNotInventOtherRoles()
    {
        var place = PlaceAggregate.CreateRestaurant("RST-MED-1", "Media Bistro", Now);
        var coverId = Guid.Parse("01900000-0000-7000-8000-000000000401");
        var galleryId = Guid.Parse("01900000-0000-7000-8000-000000000402");

        place.SetCover(coverId, Now);
        place.AddGalleryItem(galleryId, Now);
        place.RemoveCover(Now);
        place.RemoveGalleryItem(galleryId, Now);

        Assert.Null(place.Cover);
        Assert.Empty(place.GalleryOrdered);
        Assert.Equal([PlaceMediaRole.Cover, PlaceMediaRole.Gallery], Enum.GetValues<PlaceMediaRole>());
    }
}

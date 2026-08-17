using NodaTime;
using TravelCore.Modules.Ugc.Contracts;
using TravelCore.Modules.Ugc.Domain;
using Xunit;

namespace TravelCore.Modules.Ugc.UnitTests;

/// <summary>
/// UserPhoto UGC relationship (TC-P16-T005 / P16-R5). UserPhoto != MediaAsset.
/// </summary>
public sealed class UserPhotoTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 21, 0);
    private static readonly Guid Actor = Guid.Parse("0198b3e0-0000-7000-8000-000000000071");
    private static readonly Guid Media = Guid.Parse("0198b3e0-0000-7000-8000-000000000072");

    [Fact]
    public void Create_Owns_Logical_MediaAsset_Relationship()
    {
        var photo = UserPhoto.Create(Actor, Media, Now);

        Assert.NotEqual(Guid.Empty, photo.Id.Value);
        Assert.Equal(Actor, photo.ActorId);
        Assert.Equal(Media, photo.MediaAssetId);
        Assert.Equal(Now, photo.CreatedAt);
        Assert.Equal(Now, photo.UpdatedAt);
        Assert.True(UgcOwnershipBoundary.UserPhotoImplemented);
        Assert.True(UgcOwnershipBoundary.UserPhotoIsNotMediaAsset);
        Assert.False(UgcOwnershipBoundary.OwnsMediaAssetTruth);
        Assert.False(UgcOwnershipBoundary.ModerationWorkflowImplemented);
        Assert.Null(typeof(UserPhoto).GetProperty("StorageKey"));
        Assert.Null(typeof(UserPhoto).GetProperty("MimeType"));
        Assert.Null(typeof(UserPhoto).GetProperty("FileSize"));
        Assert.Null(typeof(UserPhoto).GetProperty("Width"));
        Assert.Null(typeof(UserPhoto).GetProperty("Height"));
        Assert.Null(typeof(UserPhoto).GetProperty("FocalPoint"));
        Assert.Null(typeof(UserPhoto).GetProperty("Renditions"));
        Assert.Null(typeof(UserPhoto).GetProperty("PublicationStatus"));
    }

    [Fact]
    public void Create_Rejects_Empty_Actor_Or_MediaAsset()
    {
        Assert.Throws<ArgumentException>(() => UserPhoto.Create(Guid.Empty, Media, Now));
        Assert.Throws<ArgumentException>(() => UserPhoto.Create(Actor, Guid.Empty, Now));
    }
}

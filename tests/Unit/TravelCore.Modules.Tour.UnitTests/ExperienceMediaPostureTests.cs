using NodaTime;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.Modules.Tour.UnitTests;

/// <summary>
/// TC-P10-T007: Experience media posture reuses TourProduct Cover/Gallery (P10-R4 / P09-R8).
/// </summary>
public sealed class ExperienceMediaPostureTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 9, 0);

    [Fact]
    public void ExperienceProduct_SupportsCoverAndGallery_ViaTourProductMediaLinks()
    {
        var product = TourProduct.CreateExperience("EXP-MEDIA-001", "Trail", Now);
        _ = TourExperienceSpecialization.CreateFor(product, Now);

        var coverId = Guid.Parse("aaaaaaaa-aaaa-7aaa-8aaa-aaaaaaaaaaaa");
        var galleryId = Guid.Parse("bbbbbbbb-bbbb-7bbb-8bbb-bbbbbbbbbbbb");

        product.SetCover(coverId, Now);
        product.AddGalleryItem(galleryId, Now);

        Assert.NotNull(product.Cover);
        Assert.Equal(coverId, product.Cover!.MediaAssetId);
        Assert.Equal(TourMediaRole.Cover, product.Cover.Role);
        Assert.Single(product.GalleryOrdered);
        Assert.Equal(galleryId, product.GalleryOrdered[0].MediaAssetId);
        Assert.DoesNotContain(typeof(TourExperienceSpecialization).Assembly.GetTypes(), t => t.Name == "ExperienceMediaLink");
        Assert.DoesNotContain(typeof(TourExperienceSpecialization).Assembly.GetTypes(), t => t.Name.Contains("DayMedia", StringComparison.Ordinal));
        Assert.DoesNotContain(typeof(TourExperienceSpecialization).Assembly.GetTypes(), t => t.Name.Contains("StopMedia", StringComparison.Ordinal));
    }
}

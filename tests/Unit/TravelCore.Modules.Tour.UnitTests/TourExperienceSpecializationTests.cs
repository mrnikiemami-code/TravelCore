using NodaTime;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.Modules.Tour.UnitTests;

public sealed class TourExperienceSpecializationTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 4, 0);

    [Fact]
    public void CreateFor_ExperienceProduct_AttachesWithSameId()
    {
        var product = TourProduct.CreateExperience("EXP-SPEC-001", "Caspian Walk", Now);

        var specialization = TourExperienceSpecialization.CreateFor(product, Now);

        Assert.Equal(product.Id, specialization.TourProductId);
        Assert.Equal(Now, specialization.CreatedAt);
        Assert.Equal(Now, specialization.UpdatedAt);
    }

    [Fact]
    public void CreateFor_PackageProduct_IsRejected()
    {
        var product = TourProduct.CreatePackage("PKG-SPEC-001", "Istanbul Package", Now);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            TourExperienceSpecialization.CreateFor(product, Now));

        Assert.Contains("Experience", ex.Message, StringComparison.Ordinal);
        Assert.Contains("Package", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CreateFor_NullProduct_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            TourExperienceSpecialization.CreateFor(null!, Now));
    }

    [Fact]
    public void Touch_UpdatesTimestampOnly()
    {
        var product = TourProduct.CreateExperience("EXP-SPEC-002", "Name", Now);
        var specialization = TourExperienceSpecialization.CreateFor(product, Now);
        var later = Now.Plus(Duration.FromMinutes(10));

        specialization.Touch(later);

        Assert.Equal(Now, specialization.CreatedAt);
        Assert.Equal(later, specialization.UpdatedAt);
        Assert.Equal(product.Id, specialization.TourProductId);
    }

    [Fact]
    public void Reconstitute_PreservesIdentity()
    {
        var id = TourProductId.New();
        var created = Now;
        var updated = Now.Plus(Duration.FromHours(1));

        var specialization = TourExperienceSpecialization.Reconstitute(id, created, updated);

        Assert.Equal(id, specialization.TourProductId);
        Assert.Equal(created, specialization.CreatedAt);
        Assert.Equal(updated, specialization.UpdatedAt);
    }
}

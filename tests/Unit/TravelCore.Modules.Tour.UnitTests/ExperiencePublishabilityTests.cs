using NodaTime;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.Modules.Tour.UnitTests;

public sealed class ExperiencePublishabilityTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 10, 0);

    [Fact]
    public void EnsureCanPublish_RequiresTitleCoverDestinationAndFacts()
    {
        var product = TourProduct.CreateExperience("EXP-PUB-001", "Trail", Now);
        var spec = TourExperienceSpecialization.CreateFor(product, Now);

        var reasons = ExperiencePublishability.EvaluateBlockingReasons(product, spec);
        Assert.Contains(reasons, r => r.Contains("title", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(reasons, r => r.Contains("Cover", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(reasons, r => r.Contains("Destination", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(reasons, r => r.Contains("Meaningful", StringComparison.OrdinalIgnoreCase));
        Assert.Throws<InvalidOperationException>(() =>
            ExperiencePublishability.EnsureCanPublish(product, spec));
    }

    [Fact]
    public void EnsureCanPublish_PassesWhenCatalogComplete_WithoutDepartureOrPrice()
    {
        var product = TourProduct.CreateExperience("EXP-PUB-002", "Trail", Now);
        product.UpsertTranslation("en", "Alpine Day Hike", null, Now);
        product.SetCover(Guid.Parse("aaaaaaaa-aaaa-7aaa-8aaa-aaaaaaaaaaaa"), Now);
        product.AssignDestination(Guid.Parse("bbbbbbbb-bbbb-7bbb-8bbb-bbbbbbbbbbbb"), Now);

        var spec = TourExperienceSpecialization.CreateFor(product, Now);
        spec.SetDifficulty(ExperienceDifficulty.Moderate, Now);

        Assert.Empty(ExperiencePublishability.EvaluateBlockingReasons(product, spec));
        ExperiencePublishability.EnsureCanPublish(product, spec);
        product.SetCatalogStatus(TourCatalogStatus.Published, Now);
        Assert.Equal(TourCatalogStatus.Published, product.CatalogStatus);
        Assert.DoesNotContain(typeof(TourExperienceSpecialization).Assembly.GetTypes(), t => t.Name == "ExperienceCatalogStatus");
        Assert.DoesNotContain(typeof(TourExperienceSpecialization).Assembly.GetTypes(), t => t.Name == "ExperiencePublicationState");
    }
}

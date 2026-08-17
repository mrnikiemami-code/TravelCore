using NodaTime;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.Modules.Tour.UnitTests;

public sealed class ExperienceOperationalAttributesTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 7, 0);

    [Fact]
    public void SetDifficulty_AndReplaceStructuredFacts()
    {
        var product = TourProduct.CreateExperience("EXP-OPS-001", "Trail", Now);
        var spec = TourExperienceSpecialization.CreateFor(product, Now);

        spec.SetDifficulty(ExperienceDifficulty.Moderate, Now);
        spec.ReplaceEligibilityRequirements(
            [("minimum-age", "12", null), ("pregnancy-restricted", "true", "Not recommended")],
            Now);
        spec.ReplaceEquipment(
            [
                ("hiking-shoes", ExperienceEquipmentKind.Required, null),
                ("poles", ExperienceEquipmentKind.Recommended, "Optional poles")
            ],
            Now);
        spec.ReplaceLocalTransport([("minibus", "Shared transfer"), ("walking", null)], Now);

        Assert.Equal(ExperienceDifficulty.Moderate, spec.Difficulty);
        Assert.Equal(2, spec.EligibilityRequirements.Count);
        Assert.Contains(spec.EligibilityRequirements, x => x.Code == "minimum-age" && x.Value == "12");
        Assert.Equal(2, spec.Equipment.Count);
        Assert.Contains(spec.Equipment, x => x.Code == "hiking-shoes" && x.Kind == ExperienceEquipmentKind.Required);
        Assert.Equal(2, spec.LocalTransport.Count);
        Assert.DoesNotContain(typeof(TourExperienceSpecialization).Assembly.GetTypes(), t => t.Name == "TourHotelOption");
        Assert.DoesNotContain(typeof(TourExperienceSpecialization).Assembly.GetTypes(), t => t.Name == "FlightSegment");
    }

    [Fact]
    public void SetDifficulty_RejectsUndefinedEnum()
    {
        var product = TourProduct.CreateExperience("EXP-OPS-002", "Trail", Now);
        var spec = TourExperienceSpecialization.CreateFor(product, Now);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            spec.SetDifficulty((ExperienceDifficulty)99, Now));
    }
}

using NodaTime;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.Modules.Tour.UnitTests;

public sealed class ExperienceGuideAssignmentTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 8, 0);

    [Fact]
    public void AddGuideAssignment_StoresRoleAndRejectsDuplicateParty()
    {
        var product = TourProduct.CreateExperience("EXP-GUIDE-001", "Trail", Now);
        var spec = TourExperienceSpecialization.CreateFor(product, Now);
        var primaryId = Guid.Parse("11111111-1111-7111-8111-111111111111");
        var assistantId = Guid.Parse("22222222-2222-7222-8222-222222222222");

        var primary = spec.AddGuideAssignment(primaryId, ExperienceGuideRole.Primary, Now, "Lead guide");
        var assistant = spec.AddGuideAssignment(assistantId, ExperienceGuideRole.Assistant, Now);

        Assert.Equal(2, spec.GuideAssignments.Count);
        Assert.Equal(ExperienceGuideRole.Primary, primary.Role);
        Assert.Equal("Lead guide", primary.Note);
        Assert.Equal(ExperienceGuideRole.Assistant, assistant.Role);
        Assert.Throws<ArgumentException>(() =>
            spec.AddGuideAssignment(primaryId, ExperienceGuideRole.Assistant, Now));
        Assert.DoesNotContain(typeof(TourExperienceSpecialization).Assembly.GetTypes(), t => t.Name == "Guide");
    }

    [Fact]
    public void RemoveGuideAssignment_RemovesById()
    {
        var product = TourProduct.CreateExperience("EXP-GUIDE-002", "Trail", Now);
        var spec = TourExperienceSpecialization.CreateFor(product, Now);
        var partyId = Guid.Parse("33333333-3333-7333-8333-333333333333");
        var assignment = spec.AddGuideAssignment(partyId, ExperienceGuideRole.Primary, Now);

        Assert.True(spec.RemoveGuideAssignment(assignment.Id, Now));
        Assert.Empty(spec.GuideAssignments);
        Assert.False(spec.RemoveGuideAssignment(assignment.Id, Now));
    }

    [Fact]
    public void AddGuideAssignment_RejectsEmptyPartyAndUndefinedRole()
    {
        var product = TourProduct.CreateExperience("EXP-GUIDE-003", "Trail", Now);
        var spec = TourExperienceSpecialization.CreateFor(product, Now);

        Assert.Throws<ArgumentException>(() =>
            spec.AddGuideAssignment(Guid.Empty, ExperienceGuideRole.Primary, Now));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ExperienceGuideAssignment.Reconstitute(
                ExperienceGuideAssignmentId.New(),
                product.Id,
                Guid.Parse("44444444-4444-7444-8444-444444444444"),
                (ExperienceGuideRole)99,
                null));
    }
}

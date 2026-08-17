using NodaTime;
using TravelCore.Modules.Visa.Contracts;
using TravelCore.Modules.Visa.Domain;
using Xunit;

namespace TravelCore.Modules.Visa.UnitTests;

/// <summary>
/// ProcessingTime != VisaValidity != AllowedStay != EntryPolicy (TC-P17-T005 / P17-R5).
/// </summary>
public sealed class VisaIssuanceSemanticsTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 22, 0);
    private static readonly Guid France = Guid.Parse("0198b3e0-0000-7000-8000-000000000061");

    private static VisaRequirementSet CreateSet() =>
        VisaDefinition.Create("TOURIST", "en", "Tourist", Now).AddRequirementSet(France, Now, "ir");

    [Fact]
    public void Processing_Validity_Stay_And_Entry_Are_Distinct_Facts_Not_One_Duration()
    {
        var set = CreateSet();
        var processing = set.SetProcessingTime(7, "BusinessDays", Now, 15);
        var validity = set.SetValidity(90, "Days", Now);
        var stay = set.SetAllowedStay(30, "Days", Now);
        var entry = set.SetEntryPolicy("Single", Now);

        Assert.Equal(7, processing.MinValue);
        Assert.Equal(15, processing.MaxValue);
        Assert.Equal(VisaTimeUnit.BusinessDays, processing.Unit);
        Assert.Equal(90, validity.Value);
        Assert.Equal(VisaTimeUnit.Days, validity.Unit);
        Assert.Equal(30, stay.Value);
        Assert.Equal(VisaTimeUnit.Days, stay.Unit);
        Assert.Equal(VisaEntryKind.Single, entry.Kind);
        Assert.NotEqual(typeof(VisaProcessingTime), typeof(VisaValidity));
        Assert.NotEqual(typeof(VisaProcessingTime), typeof(VisaAllowedStay));
        Assert.NotEqual(typeof(VisaValidity), typeof(VisaAllowedStay));
        Assert.NotEqual(typeof(VisaEntryPolicy), typeof(VisaProcessingTime));
        Assert.Null(typeof(VisaRequirementSet).GetProperty("Duration"));
        Assert.Null(typeof(VisaProcessingTime).GetProperty("Duration"));
        Assert.Null(typeof(VisaValidity).GetProperty("Duration"));
        Assert.Null(typeof(VisaAllowedStay).GetProperty("Duration"));
        Assert.Null(typeof(VisaProcessingTime).GetProperty("Amount"));
        Assert.Null(typeof(VisaValidity).GetProperty("Fee"));
        Assert.Null(typeof(VisaProcessingTime).GetProperty("Expression"));
        Assert.Throws<ArgumentException>(() => VisaTimeUnit.Parse("Weeks"));
        Assert.Throws<ArgumentException>(() => set.SetEntryPolicy("Unlimited", Now));
        Assert.Throws<ArgumentOutOfRangeException>(() => set.SetProcessingTime(15, "Days", Now, 7));
        Assert.True(VisaOwnershipBoundary.ProcessingValidityModelImplemented);
        Assert.True(VisaOwnershipBoundary.FeeModelImplemented);
        Assert.False(VisaOwnershipBoundary.ApplicationWorkflowImplemented);
        Assert.False(VisaOwnershipBoundary.RegulatoryEngineImplemented);
    }

    [Fact]
    public void EffectivePeriod_Is_Readiness_Not_A_Versioning_Engine()
    {
        var set = CreateSet();
        var from = Instant.FromUtc(2026, 1, 1, 0, 0);
        var to = Instant.FromUtc(2026, 12, 31, 0, 0);
        set.SetEffectivePeriod(from, to, Now);

        Assert.Equal(from, set.EffectiveFrom);
        Assert.Equal(to, set.EffectiveTo);
        Assert.Null(typeof(VisaRequirementSet).GetProperty("Version"));
        Assert.Null(typeof(VisaRequirementSet).GetProperty("SupersedesId"));
        Assert.Throws<ArgumentOutOfRangeException>(() => set.SetEffectivePeriod(to, from, Now));
        Assert.True(VisaOwnershipBoundary.FutureEffectivePeriodAllowed);
    }
}

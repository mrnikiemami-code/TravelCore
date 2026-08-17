using NodaTime;
using TravelCore.Modules.Visa.Contracts;
using TravelCore.Modules.Visa.Domain;
using Xunit;

namespace TravelCore.Modules.Visa.UnitTests;

/// <summary>
/// VisaApplicability is structured facts, not a rules engine (TC-P17-T003 / P17-R3).
/// </summary>
public sealed class VisaApplicabilityTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 21, 0);
    private static readonly Guid France = Guid.Parse("0198b3e0-0000-7000-8000-000000000061");

    [Fact]
    public void RequirementSet_Owns_Exactly_One_Logical_Applicability_Context()
    {
        var definition = VisaDefinition.Create("TOURIST", "en", "Tourist", Now);
        var set = definition.AddRequirementSet(France, Now, " ir ", " de ", "Minor");

        Assert.Same(set.Applicability, set.Applicability);
        Assert.Equal(set.Id, set.Applicability.VisaRequirementSetId);
        Assert.Equal(France, set.Applicability.DestinationGeographicId);
        Assert.Equal("IR", set.Applicability.ApplicantNationalityCode);
        Assert.Equal("DE", set.Applicability.ResidenceCountryCode);
        Assert.Equal(VisaApplicantCategory.Minor, set.Applicability.ApplicantCategory);
        Assert.Single(typeof(VisaRequirementSet).GetProperties(), p => p.Name == "Applicability");
        Assert.Null(typeof(VisaApplicability).GetProperty("Amount"));
        Assert.Null(typeof(VisaApplicability).GetProperty("Expression"));
        Assert.Null(typeof(VisaApplicability).GetProperty("Predicate"));
        Assert.Null(typeof(VisaApplicability).GetProperty("Rules"));
        Assert.False(VisaOwnershipBoundary.OwnsDestinationFacts);
        Assert.False(VisaOwnershipBoundary.OwnsReferenceData);
        Assert.False(VisaOwnershipBoundary.OwnsIdentityOrParty);
        Assert.True(VisaOwnershipBoundary.VisaApplicabilityImplemented);
        Assert.False(VisaOwnershipBoundary.ApplicabilityIsRulesEngine);
        Assert.True(VisaOwnershipBoundary.RequiredDocumentImplemented);
        Assert.True(VisaOwnershipBoundary.EligibilityModelImplemented);
        Assert.False(VisaOwnershipBoundary.EligibilityIsRulesEngine);
        Assert.NotEqual(typeof(VisaApplicability), typeof(VisaRequirementSet));
        Assert.Null(typeof(VisaApplicability).Assembly.GetType("TravelCore.Modules.Visa.Domain.Country"));
        Assert.Null(typeof(VisaApplicability).Assembly.GetType("TravelCore.Modules.Visa.Domain.Destination"));
        Assert.Null(typeof(VisaApplicability).Assembly.GetType("TravelCore.Modules.Visa.Domain.Nationality"));
    }

    [Fact]
    public void Applicability_Rejects_Empty_Destination_And_Invalid_Codes()
    {
        var definition = VisaDefinition.Create("TOURIST", "en", "Tourist", Now);
        Assert.Throws<ArgumentException>(() => definition.AddRequirementSet(Guid.Empty, Now));
        Assert.Throws<ArgumentException>(() => definition.AddRequirementSet(France, Now, "IRA"));
        Assert.Throws<ArgumentException>(() => definition.AddRequirementSet(France, Now, null, "1"));
        Assert.Throws<ArgumentException>(() => definition.AddRequirementSet(France, Now, null, null, "Child"));
        Assert.Equal(VisaApplicantCategory.Adult, VisaApplicantCategory.Parse("Adult"));
        Assert.Equal(VisaApplicantCategory.Other, VisaApplicantCategory.Parse("Other"));
        Assert.Null(VisaApplicantCategory.ParseOptional(" "));
    }
}

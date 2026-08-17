using NodaTime;
using TravelCore.Modules.Visa.Contracts;
using TravelCore.Modules.Visa.Domain;
using Xunit;

namespace TravelCore.Modules.Visa.UnitTests;

/// <summary>
/// RequiredDocument != EligibilityRequirement. Structured facts, not uploads or a rules engine (TC-P17-T004).
/// </summary>
public sealed class VisaRequirementFactTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 21, 0);
    private static readonly Guid France = Guid.Parse("0198b3e0-0000-7000-8000-000000000061");

    private static VisaRequirementSet CreateSet() =>
        VisaDefinition.Create("TOURIST", "en", "Tourist", Now).AddRequirementSet(France, Now, "ir");

    [Fact]
    public void RequiredDocument_Is_Row_Based_Fact_Not_Upload_Or_Flag_Column()
    {
        var set = CreateSet();
        var passport = set.AddRequiredDocument(" Passport ", "Required", "en", " Passport ", Now, "  Valid original  ", 1);
        set.AddRequiredDocument("photo", "Optional", "en", "Photo", Now);

        Assert.Equal(2, set.RequiredDocuments.Count);
        Assert.Equal("passport", passport.Code);
        Assert.Equal(VisaRequirementLevel.Required, passport.RequirementLevel);
        Assert.Equal(1, passport.SortOrder);
        Assert.Equal("Passport", Assert.Single(passport.Translations).Name);
        Assert.Equal("Valid original", Assert.Single(passport.Translations).Notes);
        Assert.Throws<InvalidOperationException>(() =>
            set.AddRequiredDocument("PASSPORT", "Required", "en", "Again", Now));
        Assert.Throws<ArgumentException>(() => set.AddRequiredDocument("1photo", "Required", "en", "Bad", Now));
        Assert.Null(typeof(VisaRequiredDocument).GetProperty("StorageKey"));
        Assert.Null(typeof(VisaRequiredDocument).GetProperty("MimeType"));
        Assert.Null(typeof(VisaRequiredDocument).GetProperty("FileSize"));
        Assert.Null(typeof(VisaRequirementSet).GetProperty("RequiresPassport"));
        Assert.True(VisaOwnershipBoundary.RequiredDocumentImplemented);
        Assert.False(VisaOwnershipBoundary.OwnsMediaAssetTruth);
        Assert.NotEqual(typeof(VisaRequiredDocument), typeof(VisaEligibilityRequirement));
    }

    [Fact]
    public void EligibilityRequirement_Is_Structured_Fact_Not_Rules_Engine()
    {
        var set = CreateSet();
        var validity = set.AddEligibilityRequirement(
            "passport_min_validity",
            "Required",
            "en",
            "Passport validity",
            Now,
            kind: " Validity ",
            value: " 6 ",
            unit: " months ");

        Assert.Equal("passport_min_validity", validity.Code);
        Assert.Equal("validity", validity.Kind);
        Assert.Equal("6", validity.Value);
        Assert.Equal("months", validity.Unit);
        Assert.Equal(VisaRequirementLevel.Required, validity.RequirementLevel);
        Assert.Throws<InvalidOperationException>(() =>
            set.AddEligibilityRequirement("passport_min_validity", "Required", "en", "Dup", Now));
        Assert.Throws<ArgumentException>(() => VisaRequirementLevel.Parse("Maybe"));
        Assert.Null(typeof(VisaEligibilityRequirement).GetProperty("Expression"));
        Assert.Null(typeof(VisaEligibilityRequirement).GetProperty("Predicate"));
        Assert.Null(typeof(VisaEligibilityRequirement).GetProperty("DestinationGeographicId"));
        Assert.Null(typeof(VisaEligibilityRequirement).GetProperty("ApplicantNationalityCode"));
        Assert.True(VisaOwnershipBoundary.EligibilityModelImplemented);
        Assert.False(VisaOwnershipBoundary.EligibilityIsRulesEngine);
        Assert.False(VisaOwnershipBoundary.ProcessingValidityModelImplemented);
        Assert.False(VisaOwnershipBoundary.FeeModelImplemented);
        Assert.False(VisaOwnershipBoundary.ApplicationWorkflowImplemented);
    }
}

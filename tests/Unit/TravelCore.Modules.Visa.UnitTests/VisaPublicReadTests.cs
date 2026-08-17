using NodaTime;
using TravelCore.Modules.Visa.Contracts;
using TravelCore.Modules.Visa.Domain;
using TravelCore.Modules.Visa.Infrastructure.Services;
using Xunit;

namespace TravelCore.Modules.Visa.UnitTests;

/// <summary>
/// Public Visa read contracts keep Content/SEO/Search/application out (TC-P17-T007 / P17-R7).
/// </summary>
public sealed class VisaPublicReadTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 0, 0);
    private static readonly Guid France = Guid.Parse("0198b3e0-0000-7000-8000-000000000071");

    [Fact]
    public void PublicEligibility_Is_Locale_Explicit_Not_Auto_Detected()
    {
        Assert.True(VisaPublicEligibility.HasLocaleTranslation("en", ["en", "fa"]));
        Assert.True(VisaPublicEligibility.HasLocaleTranslation("EN", ["en"]));
        Assert.False(VisaPublicEligibility.HasLocaleTranslation("fa", ["en"]));
        Assert.False(VisaPublicEligibility.HasLocaleTranslation("fa", ["fa-IR"]));
        Assert.Equal("fa-IR", VisaPublicEligibility.NormalizeLocaleCode("fa-ir"));
        Assert.Throws<ArgumentException>(() => VisaPublicEligibility.HasLocaleTranslation(" ", ["en"]));
    }

    [Fact]
    public void PublicMapper_Keeps_Documents_Eligibility_Timing_And_Fees_Distinct()
    {
        var definition = VisaDefinition.Create("TOURIST", "en", "Tourist visa", Now, "Visitor entry");
        var set = definition.AddRequirementSet(France, Now, "IR", "IR", "Adult");
        set.AddRequiredDocument("PASSPORT", "Required", "en", "Passport", Now, "Valid six months");
        set.AddEligibilityRequirement(
            "MIN_PASSPORT_VALIDITY",
            "Required",
            "en",
            "Passport validity",
            Now,
            "MinValidity",
            "6",
            "Months");
        set.SetProcessingTime(5, "BusinessDays", Now, 10);
        set.SetValidity(90, "Days", Now);
        set.SetAllowedStay(30, "Days", Now);
        set.SetEntryPolicy("Single", Now);
        set.AddOfficialFee("Embassy", 80m, "EUR", Now, "consular schedule", 1);
        set.SetEffectivePeriod(Now, Now.Plus(Duration.FromDays(30)), Now);

        var mapped = VisaPublicReadMapper.TryMap(definition, "en");
        Assert.NotNull(mapped);
        Assert.Equal("TOURIST", mapped.Code);
        Assert.Equal("en", mapped.LocaleCode);
        Assert.Equal("Tourist visa", mapped.Name);
        Assert.Equal("visas/TOURIST", mapped.PublicPath);
        var publicSet = Assert.Single(mapped.RequirementSets);
        Assert.Equal(France, publicSet.Applicability.DestinationGeographicId);
        Assert.Equal("IR", publicSet.Applicability.ApplicantNationalityCode);
        Assert.Equal("Adult", publicSet.Applicability.ApplicantCategory);
        var document = Assert.Single(publicSet.RequiredDocuments);
        Assert.Equal("passport", document.Code);
        Assert.Equal("Passport", document.Name);
        var eligibility = Assert.Single(publicSet.EligibilityRequirements);
        Assert.Equal("min_passport_validity", eligibility.Code);
        Assert.Equal("minvalidity", eligibility.Kind);
        Assert.NotNull(publicSet.ProcessingTime);
        Assert.Equal(5, publicSet.ProcessingTime.MinValue);
        Assert.Equal(10, publicSet.ProcessingTime.MaxValue);
        Assert.Equal("BusinessDays", publicSet.ProcessingTime.Unit);
        Assert.Equal(90, publicSet.Validity?.Value);
        Assert.Equal("Days", publicSet.Validity?.Unit);
        Assert.Equal(30, publicSet.AllowedStay?.Value);
        Assert.Equal("Single", publicSet.EntryPolicy?.Kind);
        var fee = Assert.Single(publicSet.OfficialFees);
        Assert.Equal("Embassy", fee.Kind);
        Assert.Equal("80", fee.Money.Amount);
        Assert.Equal("EUR", fee.Money.CurrencyCode);
        Assert.Equal("consular schedule", fee.Source);
        Assert.NotNull(publicSet.EffectiveFrom);
        Assert.NotNull(publicSet.EffectiveTo);
        Assert.Null(typeof(PublicVisaRequirementSet).GetProperty("Duration"));
        Assert.Null(typeof(PublicVisaRequirementSet).GetProperty("Price"));
        Assert.Null(typeof(PublicVisaRequirementSet).GetProperty("Quote"));
        Assert.Null(mapped.GetType().GetProperty("IndexPolicy"));
        Assert.Null(VisaPublicReadMapper.TryMap(definition, "fa"));
    }

    [Fact]
    public void PublicComposition_Does_Not_Steal_Content_Seo_Search_Or_Application()
    {
        Assert.Equal("Visa", VisaPublicCompositionBoundary.FactOwner);
        Assert.Equal("PublicExperience", VisaPublicCompositionBoundary.PresentationOwner);
        Assert.Equal("Content", VisaPublicCompositionBoundary.EditorialOwner);
        Assert.Equal("Seo", VisaPublicCompositionBoundary.IndexPolicyOwner);
        Assert.Equal("Search", VisaPublicCompositionBoundary.SearchOwner);
        Assert.Equal("/visas/{code}", VisaPublicCompositionBoundary.PublicRoutePattern);
        Assert.False(VisaPublicCompositionBoundary.PublicPresenceEqualsSeoIndexed);
        Assert.False(VisaPublicCompositionBoundary.PublicPresenceEqualsAutomaticallySearchIndexed);
        Assert.False(VisaPublicCompositionBoundary.CopyContentIntoVisaAllowed);
        Assert.False(VisaPublicCompositionBoundary.VisaOwnsIndexPolicy);
        Assert.False(VisaPublicCompositionBoundary.VisaOwnsSearch);
        Assert.False(VisaPublicCompositionBoundary.ApplicationWorkflowAllowed);
        Assert.False(VisaPublicCompositionBoundary.CommercialPriceDisplayAllowed);
        Assert.False(VisaPublicCompositionBoundary.FxConversionAllowed);
        Assert.True(VisaOwnershipBoundary.PublicReadImplemented);
        Assert.False(VisaOwnershipBoundary.PublicPresenceEqualsSeoIndexed);
        Assert.False(VisaOwnershipBoundary.ApplicationWorkflowImplemented);
        Assert.False(VisaOwnershipBoundary.OwnsIndexPolicy);
        Assert.False(VisaOwnershipBoundary.OwnsContentCms);
        Assert.False(VisaOwnershipBoundary.OwnsSearch);
    }
}

using NodaTime;
using TravelCore.Modules.Visa.Domain;
using Xunit;

namespace TravelCore.Modules.Visa.UnitTests;

/// <summary>
/// VisaDefinition vs VisaRequirementSet domain baseline (TC-P17-T002 / P17-R2).
/// </summary>
public sealed class VisaDefinitionTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 21, 0);

    [Fact]
    public void Create_Owns_Normalized_Code_And_Locale_Row_Not_Language_Columns()
    {
        var definition = VisaDefinition.Create(" tourist ", "en-US", "  Tourist visa  ", Now, "  Short stay  ");

        Assert.NotEqual(Guid.Empty, definition.Id.Value);
        Assert.Equal("TOURIST", definition.Code);
        Assert.Equal(Now, definition.CreatedAt);
        Assert.Equal(Now, definition.UpdatedAt);
        var translation = Assert.Single(definition.Translations);
        Assert.Equal(definition.Id, translation.VisaDefinitionId);
        Assert.Equal("en-US", translation.LocaleCode);
        Assert.Equal("Tourist visa", translation.Name);
        Assert.Equal("Short stay", translation.Summary);
        Assert.Null(typeof(VisaDefinition).GetProperty("NameFa"));
        Assert.Null(typeof(VisaDefinition).GetProperty("NameEn"));
        Assert.Null(typeof(VisaDefinition).GetProperty("Slug"));
        Assert.Empty(definition.RequirementSets);
        Assert.NotEqual(typeof(VisaDefinition), typeof(VisaRequirementSet));
    }

    [Fact]
    public void AddRequirementSet_Is_Same_Schema_Child_And_Does_Not_Merge_Into_Definition()
    {
        var definition = VisaDefinition.Create("BUSINESS", "fa", "ویزای تجاری", Now);
        var france = Guid.Parse("0198b3e0-0000-7000-8000-000000000061");
        var turkey = Guid.Parse("0198b3e0-0000-7000-8000-000000000062");
        var later = Instant.FromUtc(2026, 8, 17, 22, 0);
        var latest = Instant.FromUtc(2026, 8, 17, 23, 0);
        var first = definition.AddRequirementSet(france, later, "ir", null, "Adult");
        var second = definition.AddRequirementSet(turkey, latest);

        Assert.Equal(2, definition.RequirementSets.Count);
        Assert.Equal(definition.Id, first.VisaDefinitionId);
        Assert.Equal(definition.Id, second.VisaDefinitionId);
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(latest, definition.UpdatedAt);
        Assert.Equal(france, first.Applicability.DestinationGeographicId);
        Assert.Equal("IR", first.Applicability.ApplicantNationalityCode);
        Assert.Equal(VisaApplicantCategory.Adult, first.Applicability.ApplicantCategory);
        Assert.Null(second.Applicability.ApplicantNationalityCode);
        Assert.Null(typeof(VisaRequirementSet).GetProperty("Amount"));
        Assert.Null(typeof(VisaRequirementSet).GetProperty("Currency"));
        Assert.Null(typeof(VisaRequirementSet).GetProperty("Fee"));
        Assert.Null(typeof(VisaRequirementSet).GetProperty("Price"));
        Assert.Null(typeof(VisaRequirementSet).GetProperty("DestinationId"));
        Assert.Null(typeof(VisaRequirementSet).GetProperty("Nationality"));
        Assert.Null(typeof(VisaRequirementSet).GetProperty("ApplicantNationality"));
        Assert.Null(typeof(VisaRequirementSet).GetProperty("CountryOfResidence"));
        Assert.Null(typeof(VisaRequirementSet).GetProperty("RequiredDocument"));
        Assert.Null(typeof(VisaRequirementSet).GetProperty("ProcessingDuration"));
        Assert.Null(typeof(VisaDefinition).Assembly.GetType("TravelCore.Modules.Visa.Domain.VisaRequirement"));
    }

    [Fact]
    public void AddTranslation_Rejects_Duplicate_Locale_And_Invalid_Code()
    {
        var definition = VisaDefinition.Create("TRANSIT", "en", "Transit", Now);
        definition.AddTranslation("fa-IR", "ترانزیت", null, Instant.FromUtc(2026, 8, 17, 22, 0));
        Assert.Equal(2, definition.Translations.Count);
        Assert.Throws<InvalidOperationException>(() => definition.AddTranslation("EN", "Transit copy", null, Now));
        Assert.Equal("TOURIST", VisaDefinition.NormalizeCode("Tourist"));
        Assert.Throws<ArgumentException>(() => VisaDefinition.NormalizeCode("TOUR IST"));
        Assert.Throws<ArgumentException>(() => VisaDefinition.NormalizeCode(new string('A', VisaDefinition.CodeMaxLength + 1)));
        Assert.Throws<ArgumentException>(() => VisaDefinition.Create("OK", "en", " ", Now));
        Assert.Throws<ArgumentException>(() => VisaDefinition.Create("OK", "en", "Name", default));
    }
}

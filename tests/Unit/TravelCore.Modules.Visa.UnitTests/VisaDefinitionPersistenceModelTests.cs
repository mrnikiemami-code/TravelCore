using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Visa.Domain;
using TravelCore.Modules.Visa.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Xunit;

namespace TravelCore.Modules.Visa.UnitTests;

/// <summary>
/// Persistence model for VisaDefinition 1..N VisaRequirementSet (TC-P17-T002). Schema visa only; no peer FK; no fees.
/// </summary>
public sealed class VisaDefinitionPersistenceModelTests
{
    [Fact]
    public void VisaModel_Maps_Definition_And_RequirementSet_Without_Peer_Fk_Or_Fee_Columns()
    {
        using var db = new VisaDbContext(
            new DbContextOptionsBuilder<VisaDbContext>()
                .UseTravelCorePostgreSql(
                    "Host=127.0.0.1;Database=travelcore_visa_definition_model_probe;Username=x;Password=x",
                    migrationsHistorySchema: VisaDbContext.SchemaName)
                .Options);

        var model = db.Model;
        Assert.Equal("visa", model.GetDefaultSchema());

        var definitionType = model.FindEntityType(typeof(VisaDefinition));
        Assert.NotNull(definitionType);
        Assert.Equal("visa_definitions", definitionType.GetTableName());
        Assert.Equal(VisaDbContext.SchemaName, definitionType.GetSchema());
        Assert.Equal("code", definitionType.FindProperty(nameof(VisaDefinition.Code))!.GetColumnName());
        Assert.Contains(
            definitionType.GetIndexes(),
            i => i.GetDatabaseName() == "ux_visa_definitions_code" && i.IsUnique);
        Assert.Null(definitionType.FindProperty("Amount"));
        Assert.Null(definitionType.FindProperty("Currency"));
        Assert.Null(definitionType.FindProperty("Fee"));
        Assert.Null(definitionType.FindProperty("Price"));
        Assert.Null(definitionType.FindProperty("Slug"));

        var translationType = model.FindEntityType(typeof(VisaDefinitionTranslation));
        Assert.NotNull(translationType);
        Assert.Equal("visa_definition_translations", translationType.GetTableName());
        Assert.Equal(VisaDbContext.SchemaName, translationType.GetSchema());
        Assert.Equal("locale_code", translationType.FindProperty(nameof(VisaDefinitionTranslation.LocaleCode))!.GetColumnName());
        Assert.Equal("name", translationType.FindProperty(nameof(VisaDefinitionTranslation.Name))!.GetColumnName());
        Assert.Null(translationType.FindProperty("Slug"));
        var translationFk = translationType.GetForeignKeys().Single();
        Assert.Equal(typeof(VisaDefinition), translationFk.PrincipalEntityType.ClrType);
        Assert.Equal(VisaDbContext.SchemaName, translationFk.PrincipalEntityType.GetSchema());
        Assert.Equal(DeleteBehavior.Cascade, translationFk.DeleteBehavior);

        var setType = model.FindEntityType(typeof(VisaRequirementSet));
        Assert.NotNull(setType);
        Assert.Equal("visa_requirement_sets", setType.GetTableName());
        Assert.Equal(VisaDbContext.SchemaName, setType.GetSchema());
        Assert.Equal(
            "visa_definition_id",
            setType.FindProperty(nameof(VisaRequirementSet.VisaDefinitionId))!.GetColumnName());
        Assert.Null(setType.FindProperty("Amount"));
        Assert.Null(setType.FindProperty("Currency"));
        Assert.Null(setType.FindProperty("Fee"));
        Assert.Null(setType.FindProperty("Price"));
        Assert.Null(setType.FindProperty("DestinationId"));
        Assert.Null(setType.FindProperty("Nationality"));
        Assert.Null(setType.FindProperty("ApplicantNationality"));
        Assert.Null(setType.FindProperty("CountryOfResidence"));
        var setFk = setType.GetForeignKeys().Single();
        Assert.Equal(typeof(VisaDefinition), setFk.PrincipalEntityType.ClrType);
        Assert.Equal(VisaDbContext.SchemaName, setFk.PrincipalEntityType.GetSchema());
        Assert.Equal(DeleteBehavior.Cascade, setFk.DeleteBehavior);
        Assert.Contains(
            setType.GetIndexes(),
            i => i.GetDatabaseName() == "ix_visa_requirement_sets_visa_definition_id");

        var applicabilityType = model.FindEntityType(typeof(VisaApplicability));
        Assert.NotNull(applicabilityType);
        Assert.Equal("visa_applicabilities", applicabilityType.GetTableName());
        Assert.Equal(VisaDbContext.SchemaName, applicabilityType.GetSchema());
        Assert.Equal(
            "destination_geographic_id",
            applicabilityType.FindProperty(nameof(VisaApplicability.DestinationGeographicId))!.GetColumnName());
        Assert.Equal(
            "applicant_nationality_code",
            applicabilityType.FindProperty(nameof(VisaApplicability.ApplicantNationalityCode))!.GetColumnName());
        Assert.Equal(
            "residence_country_code",
            applicabilityType.FindProperty(nameof(VisaApplicability.ResidenceCountryCode))!.GetColumnName());
        Assert.Null(applicabilityType.FindProperty("Expression"));
        Assert.Null(applicabilityType.FindProperty("Predicate"));
        Assert.Null(applicabilityType.FindProperty("Rules"));
        Assert.Null(applicabilityType.FindProperty("Amount"));
        var applicabilityFk = applicabilityType.GetForeignKeys().Single();
        Assert.Equal(typeof(VisaRequirementSet), applicabilityFk.PrincipalEntityType.ClrType);
        Assert.Equal(VisaDbContext.SchemaName, applicabilityFk.PrincipalEntityType.GetSchema());
        Assert.Equal(DeleteBehavior.Cascade, applicabilityFk.DeleteBehavior);

        Assert.Equal(12, model.GetEntityTypes().Count());
        Assert.DoesNotContain(
            model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()),
            f =>
            {
                var schema = f.PrincipalEntityType.GetSchema();
                return schema is "identity" or "party" or "tour" or "place" or "destination"
                    or "reference_data" or "content" or "media" or "seo" or "search"
                    or "pricing" or "ugc" or "agency_marketplace";
            });

        var columns = model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Select(p => p.GetColumnName())
            .Where(n => n is not null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("amount", columns);
        Assert.DoesNotContain("currency", columns);
        Assert.DoesNotContain("fee", columns);
        Assert.DoesNotContain("price", columns);
        Assert.DoesNotContain("destination_id", columns);
        Assert.DoesNotContain("nationality", columns);
        Assert.DoesNotContain("storage_key", columns);
        Assert.DoesNotContain("mime_type", columns);
        Assert.DoesNotContain("file_size", columns);
        Assert.DoesNotContain("duration", columns);
        Assert.NotNull(model.FindEntityType(typeof(VisaProcessingTime)));
        Assert.Equal("visa_processing_times", model.FindEntityType(typeof(VisaProcessingTime))!.GetTableName());
        Assert.NotNull(model.FindEntityType(typeof(VisaValidity)));
        Assert.Equal("visa_validities", model.FindEntityType(typeof(VisaValidity))!.GetTableName());
        Assert.NotNull(model.FindEntityType(typeof(VisaAllowedStay)));
        Assert.Equal("visa_allowed_stays", model.FindEntityType(typeof(VisaAllowedStay))!.GetTableName());
        Assert.NotNull(model.FindEntityType(typeof(VisaEntryPolicy)));
        Assert.Equal("visa_entry_policies", model.FindEntityType(typeof(VisaEntryPolicy))!.GetTableName());
        Assert.NotNull(setType.FindProperty(nameof(VisaRequirementSet.EffectiveFrom)));
        Assert.NotNull(setType.FindProperty(nameof(VisaRequirementSet.EffectiveTo)));
        Assert.Null(setType.FindProperty("Duration"));
        Assert.Null(model.GetEntityTypes().FirstOrDefault(e =>
            string.Equals(e.GetTableName(), "visa_requirements", StringComparison.OrdinalIgnoreCase)));
        Assert.Null(model.GetEntityTypes().FirstOrDefault(e =>
            string.Equals(e.ClrType.Name, "VisaRequirement", StringComparison.Ordinal)));
        Assert.False(db.Database.HasPendingModelChanges());
    }
}

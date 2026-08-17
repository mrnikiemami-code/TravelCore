using System.Globalization;
using TravelCore.Modules.Visa.Contracts;
using TravelCore.Modules.Visa.Domain;

namespace TravelCore.Modules.Visa.Infrastructure.Services;

/// <summary>
/// Maps Visa-owned aggregates to public read contracts (TC-P17-T007 / P17-R7).
/// Locale-explicit. Does not invent editorial copy, IndexPolicy, Search, or application workflow.
/// </summary>
internal static class VisaPublicReadMapper
{
    public static PublicVisaDefinition? TryMap(VisaDefinition definition, string localeCode)
    {
        ArgumentNullException.ThrowIfNull(definition);
        var locale = VisaPublicEligibility.NormalizeLocaleCode(localeCode);
        var translation = definition.Translations.FirstOrDefault(t =>
            string.Equals(t.LocaleCode, locale, StringComparison.OrdinalIgnoreCase));
        if (translation is null)
        {
            return null;
        }

        var sets = definition.RequirementSets
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id.Value)
            .Select(set => MapSet(set, locale))
            .ToList();

        return new PublicVisaDefinition(
            definition.Id.Value,
            definition.Code,
            locale,
            translation.Name,
            translation.Summary,
            $"visas/{definition.Code}",
            sets);
    }

    private static PublicVisaRequirementSet MapSet(VisaRequirementSet set, string locale)
    {
        var applicability = set.Applicability;
        return new PublicVisaRequirementSet(
            set.Id.Value,
            new PublicVisaApplicability(
                applicability.DestinationGeographicId,
                applicability.ApplicantNationalityCode,
                applicability.ResidenceCountryCode,
                applicability.ApplicantCategory?.Value),
            set.RequiredDocuments
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Code, StringComparer.Ordinal)
                .Select(document => MapDocument(document, locale))
                .ToList(),
            set.EligibilityRequirements
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Code, StringComparer.Ordinal)
                .Select(requirement => MapEligibility(requirement, locale))
                .ToList(),
            set.ProcessingTime is null
                ? null
                : new PublicVisaProcessingTime(
                    set.ProcessingTime.MinValue,
                    set.ProcessingTime.MaxValue,
                    set.ProcessingTime.Unit.Value),
            set.Validity is null
                ? null
                : new PublicVisaValidity(set.Validity.Value, set.Validity.Unit.Value),
            set.AllowedStay is null
                ? null
                : new PublicVisaAllowedStay(set.AllowedStay.Value, set.AllowedStay.Unit.Value),
            set.EntryPolicy is null
                ? null
                : new PublicVisaEntryPolicy(set.EntryPolicy.Kind.Value),
            set.OfficialFees
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Kind.Value, StringComparer.Ordinal)
                .Select(MapFee)
                .ToList(),
            set.EffectiveFrom?.ToDateTimeOffset(),
            set.EffectiveTo?.ToDateTimeOffset());
    }

    private static PublicVisaRequiredDocument MapDocument(VisaRequiredDocument document, string locale)
    {
        var translation = document.Translations.FirstOrDefault(t =>
            string.Equals(t.LocaleCode, locale, StringComparison.OrdinalIgnoreCase));
        return new PublicVisaRequiredDocument(
            document.Id.Value,
            document.Code,
            document.RequirementLevel.Value,
            document.SortOrder,
            translation?.Name,
            translation?.Notes);
    }

    private static PublicVisaEligibilityRequirement MapEligibility(
        VisaEligibilityRequirement requirement,
        string locale)
    {
        var translation = requirement.Translations.FirstOrDefault(t =>
            string.Equals(t.LocaleCode, locale, StringComparison.OrdinalIgnoreCase));
        return new PublicVisaEligibilityRequirement(
            requirement.Id.Value,
            requirement.Code,
            requirement.RequirementLevel.Value,
            requirement.Kind,
            requirement.Value,
            requirement.Unit,
            requirement.SortOrder,
            translation?.Name,
            translation?.Notes);
    }

    private static PublicVisaOfficialFee MapFee(VisaOfficialFee fee) =>
        new(
            fee.Id.Value,
            fee.Kind.Value,
            new PublicVisaMoney(
                fee.Money.Amount.ToString(CultureInfo.InvariantCulture),
                fee.Money.Currency.Value),
            fee.SortOrder,
            fee.Source);
}

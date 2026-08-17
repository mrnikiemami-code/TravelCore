namespace TravelCore.Modules.Visa.Contracts;

/// <summary>
/// Engine-neutral public Visa read (TC-P17-T007 / P17-R7).
/// Visa owns structured facts. PublicExperience composes. Consumers must not query Visa persistence.
/// Public presence != SEO Indexed and != automatically Search indexed.
/// </summary>
public static class VisaPublicEligibility
{
    public static bool HasLocaleTranslation(string requestedLocale, IEnumerable<string> availableLocales)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestedLocale);
        ArgumentNullException.ThrowIfNull(availableLocales);

        var locale = NormalizeLocaleCode(requestedLocale);
        return availableLocales.Any(available =>
            string.Equals(NormalizeLocaleCode(available), locale, StringComparison.OrdinalIgnoreCase));
    }

    public static string NormalizeLocaleCode(string localeCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localeCode);
        var trimmed = localeCode.Trim();
        if (trimmed.Length > 16)
        {
            throw new ArgumentException("Locale code max length is 16.", nameof(localeCode));
        }

        var parts = trimmed.Split('-', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 1)
        {
            return parts[0].ToLowerInvariant();
        }

        return $"{parts[0].ToLowerInvariant()}-{parts[1].ToUpperInvariant()}";
    }
}

public sealed record PublicVisaMoney(string Amount, string CurrencyCode);

public sealed record PublicVisaApplicability(
    Guid DestinationGeographicId,
    string? ApplicantNationalityCode,
    string? ResidenceCountryCode,
    string? ApplicantCategory);

public sealed record PublicVisaRequiredDocument(
    Guid RequiredDocumentId,
    string Code,
    string RequirementLevel,
    int SortOrder,
    string? Name,
    string? Notes);

public sealed record PublicVisaEligibilityRequirement(
    Guid EligibilityRequirementId,
    string Code,
    string RequirementLevel,
    string? Kind,
    string? Value,
    string? Unit,
    int SortOrder,
    string? Name,
    string? Notes);

public sealed record PublicVisaProcessingTime(int MinValue, int? MaxValue, string Unit);

public sealed record PublicVisaValidity(int Value, string Unit);

public sealed record PublicVisaAllowedStay(int Value, string Unit);

public sealed record PublicVisaEntryPolicy(string Kind);

public sealed record PublicVisaOfficialFee(
    Guid OfficialFeeId,
    string Kind,
    PublicVisaMoney Money,
    int SortOrder,
    string? Source);

public sealed record PublicVisaRequirementSet(
    Guid RequirementSetId,
    PublicVisaApplicability Applicability,
    IReadOnlyList<PublicVisaRequiredDocument> RequiredDocuments,
    IReadOnlyList<PublicVisaEligibilityRequirement> EligibilityRequirements,
    PublicVisaProcessingTime? ProcessingTime,
    PublicVisaValidity? Validity,
    PublicVisaAllowedStay? AllowedStay,
    PublicVisaEntryPolicy? EntryPolicy,
    IReadOnlyList<PublicVisaOfficialFee> OfficialFees,
    DateTimeOffset? EffectiveFrom,
    DateTimeOffset? EffectiveTo);

public sealed record PublicVisaDefinition(
    Guid VisaDefinitionId,
    string Code,
    string LocaleCode,
    string Name,
    string? Summary,
    string PublicPath,
    IReadOnlyList<PublicVisaRequirementSet> RequirementSets);

public interface IVisaPublicQuery
{
    Task<PublicVisaDefinition?> GetByCodeAsync(
        string code,
        string localeCode,
        CancellationToken cancellationToken = default);
}

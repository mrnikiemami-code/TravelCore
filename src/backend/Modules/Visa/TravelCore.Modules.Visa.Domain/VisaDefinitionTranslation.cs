using NodaTime;

namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Locale-specific name/summary for a VisaDefinition. Locale rows only — never per-language columns.
/// Locale codes are ReferenceData-owned; Visa stores the opaque code only (no cross-schema FK).
/// </summary>
public sealed class VisaDefinitionTranslation
{
    public const int LocaleCodeMaxLength = 16;
    public const int NameMaxLength = 200;
    public const int SummaryMaxLength = 2000;

    private VisaDefinitionTranslation()
    {
        LocaleCode = null!;
        Name = null!;
    }

    private VisaDefinitionTranslation(
        VisaDefinitionId visaDefinitionId,
        string localeCode,
        string name,
        string? summary,
        Instant updatedAt)
    {
        VisaDefinitionId = visaDefinitionId;
        LocaleCode = localeCode;
        Name = name;
        Summary = summary;
        UpdatedAt = updatedAt;
    }

    public VisaDefinitionId VisaDefinitionId { get; private set; }

    public string LocaleCode { get; private set; }

    public string Name { get; private set; }

    public string? Summary { get; private set; }

    public Instant UpdatedAt { get; private set; }

    internal static VisaDefinitionTranslation Create(
        VisaDefinitionId visaDefinitionId,
        string localeCode,
        string name,
        string? summary,
        Instant now)
    {
        return new VisaDefinitionTranslation(
            visaDefinitionId,
            NormalizeLocaleCode(localeCode),
            NormalizeName(name),
            NormalizeSummary(summary),
            now);
    }

    public static string NormalizeLocaleCode(string localeCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localeCode);
        var trimmed = localeCode.Trim();
        if (trimmed.Length > LocaleCodeMaxLength)
        {
            throw new ArgumentException($"Locale code max length is {LocaleCodeMaxLength}.", nameof(localeCode));
        }

        var parts = trimmed.Split('-', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 1)
        {
            return parts[0].ToLowerInvariant();
        }

        return $"{parts[0].ToLowerInvariant()}-{parts[1].ToUpperInvariant()}";
    }

    private static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var trimmed = name.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"Visa definition name max length is {NameMaxLength}.", nameof(name));
        }

        return trimmed;
    }

    private static string? NormalizeSummary(string? summary)
    {
        if (summary is null)
        {
            return null;
        }

        var trimmed = summary.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        if (trimmed.Length > SummaryMaxLength)
        {
            throw new ArgumentException(
                $"Visa definition summary max length is {SummaryMaxLength}.",
                nameof(summary));
        }

        return trimmed;
    }
}

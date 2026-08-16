using NodaTime;

namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// TourProduct shared-core aggregate root (P09-R1).
/// Shared Tour facts live here; Experience/Package specialty tables are deferred (P09-R7 → P10/P11).
/// TourDeparture is a separate future aggregate (P11) — never collapsed into TourProduct.
/// Localized title/description: TourProductTranslation rows (TC-P09-T003 / ADR 0008). Slug deferred (P09-R5).
/// Classification / agency / media / publishing belong to later P09 tasks.
/// </summary>
public sealed class TourProduct
{
    public const int CodeMaxLength = 64;
    public const int NameMaxLength = 200;

    private readonly List<TourProductTranslation> _translations = [];

    private TourProduct()
    {
        Code = null!;
        EnglishName = null!;
    }

    private TourProduct(
        TourProductId id,
        TourKind kind,
        string code,
        string englishName,
        Instant createdAt)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(id));
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported TourKind.");
        }

        Id = id;
        Kind = kind;
        Code = code;
        EnglishName = englishName;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public TourProductId Id { get; private set; }

    public TourKind Kind { get; private set; }

    /// <summary>Stable opaque tour product code within TravelCore (not SEO slug).</summary>
    public string Code { get; private set; }

    /// <summary>Baseline English display name (localized titles live in translation rows).</summary>
    public string EnglishName { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public IReadOnlyCollection<TourProductTranslation> Translations => _translations;

    public static TourProduct CreateExperience(
        string code,
        string englishName,
        Instant now,
        TourProductId? id = null)
        => Create(TourKind.Experience, code, englishName, now, id);

    public static TourProduct CreatePackage(
        string code,
        string englishName,
        Instant now,
        TourProductId? id = null)
        => Create(TourKind.Package, code, englishName, now, id);

    /// <summary>
    /// Reconstitute a TourProduct (tests / guarded composition).
    /// </summary>
    public static TourProduct Reconstitute(
        TourProductId id,
        TourKind kind,
        string code,
        string englishName,
        Instant createdAt,
        Instant updatedAt)
    {
        var product = new TourProduct(
            id,
            kind,
            NormalizeCode(code),
            NormalizeName(englishName),
            createdAt)
        {
            UpdatedAt = updatedAt
        };
        return product;
    }

    public void RenameEnglishName(string englishName, Instant now)
    {
        EnglishName = NormalizeName(englishName);
        UpdatedAt = now;
    }

    public TourProductTranslation UpsertTranslation(
        string localeCode,
        string title,
        string? description,
        Instant now)
    {
        var normalizedLocale = TourProductTranslation.NormalizeLocaleCode(localeCode);
        var existing = _translations.FirstOrDefault(x =>
            string.Equals(x.LocaleCode, normalizedLocale, StringComparison.Ordinal));

        if (existing is null)
        {
            var created = TourProductTranslation.Create(Id, normalizedLocale, title, description, now);
            _translations.Add(created);
            UpdatedAt = now;
            return created;
        }

        existing.Update(title, description, now);
        UpdatedAt = now;
        return existing;
    }

    public TourProductTranslation? FindTranslation(string localeCode)
    {
        var normalizedLocale = TourProductTranslation.NormalizeLocaleCode(localeCode);
        return _translations.FirstOrDefault(x =>
            string.Equals(x.LocaleCode, normalizedLocale, StringComparison.Ordinal));
    }

    private static TourProduct Create(
        TourKind kind,
        string code,
        string englishName,
        Instant now,
        TourProductId? id)
    {
        return new TourProduct(
            id ?? TourProductId.New(),
            kind,
            NormalizeCode(code),
            NormalizeName(englishName),
            now);
    }

    public static string NormalizeCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        var trimmed = code.Trim();
        if (trimmed.Length > CodeMaxLength)
        {
            throw new ArgumentException($"Tour product code max length is {CodeMaxLength}.", nameof(code));
        }

        return trimmed;
    }

    public static string NormalizeName(string englishName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(englishName);
        var trimmed = englishName.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"Tour product name max length is {NameMaxLength}.", nameof(englishName));
        }

        return trimmed;
    }
}

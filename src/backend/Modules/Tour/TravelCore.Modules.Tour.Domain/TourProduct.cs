using NodaTime;

namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// TourProduct shared-core aggregate root (P09-R1).
/// Shared Tour facts live here; Experience/Package specialty tables are deferred (P09-R7 → P10/P11).
/// TourDeparture is a separate future aggregate (P11) — never collapsed into TourProduct.
/// Localized title/description: TourProductTranslation rows (TC-P09-T003 / ADR 0008). Slug deferred (P09-R5).
/// Classification + Origin (0..1) + Destination links (0..N) — TC-P09-T004 / P09-R2.
/// Agency / media / publishing belong to later P09 tasks.
/// </summary>
public sealed class TourProduct
{
    public const int CodeMaxLength = 64;
    public const int NameMaxLength = 200;
    public const int ClassificationCodeMaxLength = 64;

    private readonly List<TourProductTranslation> _translations = [];
    private readonly List<TourProductDestination> _destinations = [];
    private readonly List<TourProductService> _services = [];
    private readonly List<TourProductPolicy> _policies = [];
    private readonly List<TourProductRequirement> _requirements = [];

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

    /// <summary>
    /// Tour-owned opaque classification code (catalog facet). Not TourKind and not a lookup-owned FK.
    /// </summary>
    public string? ClassificationCode { get; private set; }

    /// <summary>
    /// Optional logical Origin Destination identity (0..1). Distinct from Destinations join (P09-R2).
    /// </summary>
    public Guid? OriginDestinationId { get; private set; }

    /// <summary>
    /// Optional logical Agency Party identity (0..1; P09-R3). Party remains SoR — never an EF navigation.
    /// </summary>
    public Guid? AgencyId { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public IReadOnlyCollection<TourProductTranslation> Translations => _translations;

    public IReadOnlyCollection<TourProductDestination> Destinations => _destinations;

    public IReadOnlyCollection<TourProductService> Services => _services;

    public IReadOnlyCollection<TourProductPolicy> Policies => _policies;

    public IReadOnlyCollection<TourProductRequirement> Requirements => _requirements;

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

    public void SetClassificationCode(string? classificationCode, Instant now)
    {
        ClassificationCode = NormalizeClassificationCode(classificationCode);
        UpdatedAt = now;
    }

    /// <summary>
    /// Sets Origin Destination logical link (0..1). Null clears; empty Guid rejected.
    /// </summary>
    public void SetOriginLink(Guid? originDestinationId, Instant now)
    {
        if (originDestinationId == Guid.Empty)
        {
            throw new ArgumentException(
                "OriginDestinationId cannot be empty. Use null to clear the Origin link.",
                nameof(originDestinationId));
        }

        OriginDestinationId = originDestinationId;
        UpdatedAt = now;
    }

    /// <summary>
    /// Sets Agency logical link (0..1; P09-R3). Null clears; empty Guid rejected.
    /// </summary>
    public void SetAgencyLink(Guid? agencyId, Instant now)
    {
        if (agencyId == Guid.Empty)
        {
            throw new ArgumentException(
                "AgencyId cannot be empty. Use null to clear the Agency link.",
                nameof(agencyId));
        }

        AgencyId = agencyId;
        UpdatedAt = now;
    }

    public TourProductDestination AssignDestination(Guid destinationId, Instant now)
    {
        if (destinationId == Guid.Empty)
        {
            throw new ArgumentException("DestinationId cannot be empty.", nameof(destinationId));
        }

        var existing = _destinations.FirstOrDefault(x => x.DestinationId == destinationId);
        if (existing is not null)
        {
            return existing;
        }

        if (_destinations.Count >= TourProductDestination.MaxLinksPerTourProduct)
        {
            throw new InvalidOperationException(
                $"A TourProduct may have at most {TourProductDestination.MaxLinksPerTourProduct} Destination links.");
        }

        var link = TourProductDestination.Create(Id, destinationId);
        _destinations.Add(link);
        UpdatedAt = now;
        return link;
    }

    public bool RemoveDestination(Guid destinationId, Instant now)
    {
        var existing = _destinations.FirstOrDefault(x => x.DestinationId == destinationId);
        if (existing is null)
        {
            return false;
        }

        _destinations.Remove(existing);
        UpdatedAt = now;
        return true;
    }

    public void ReplaceServices(IEnumerable<TourCatalogFactInput> services, Instant now)
    {
        ReplaceCatalogFacts(
            services,
            _services,
            static (id, code, detail) => TourProductService.Create(id, code, detail),
            now);
    }

    public void ReplacePolicies(IEnumerable<TourCatalogFactInput> policies, Instant now)
    {
        ReplaceCatalogFacts(
            policies,
            _policies,
            static (id, code, detail) => TourProductPolicy.Create(id, code, detail),
            now);
    }

    public void ReplaceRequirements(IEnumerable<TourCatalogFactInput> requirements, Instant now)
    {
        ReplaceCatalogFacts(
            requirements,
            _requirements,
            static (id, code, detail) => TourProductRequirement.Create(id, code, detail),
            now);
    }

    private void ReplaceCatalogFacts<T>(
        IEnumerable<TourCatalogFactInput> inputs,
        List<T> target,
        Func<TourProductId, string, string?, T> factory,
        Instant now)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var normalized = inputs
            .Select(x => (
                Code: TourCatalogFactCode.NormalizeCode(x.Code),
                Detail: TourCatalogFactCode.NormalizeDetail(x.Detail)))
            .GroupBy(x => x.Code, StringComparer.Ordinal)
            .Select(g => g.Last())
            .OrderBy(x => x.Code, StringComparer.Ordinal)
            .ToList();

        if (normalized.Count > TourCatalogFactCode.MaxEntriesPerKind)
        {
            throw new ArgumentException(
                $"A TourProduct may have at most {TourCatalogFactCode.MaxEntriesPerKind} entries of this kind.",
                nameof(inputs));
        }

        target.Clear();
        foreach (var row in normalized)
        {
            target.Add(factory(Id, row.Code, row.Detail));
        }

        UpdatedAt = now;
    }

    public static string? NormalizeClassificationCode(string? classificationCode)
    {
        if (string.IsNullOrWhiteSpace(classificationCode))
        {
            return null;
        }

        var trimmed = classificationCode.Trim().ToLowerInvariant();
        if (trimmed.Length > ClassificationCodeMaxLength)
        {
            throw new ArgumentException(
                $"Classification code max length is {ClassificationCodeMaxLength}.",
                nameof(classificationCode));
        }

        if (trimmed.Any(static c => !(char.IsAsciiLetterOrDigit(c) || c is '-' or '_')))
        {
            throw new ArgumentException(
                "Classification code may contain only a-z, 0-9, hyphen, and underscore.",
                nameof(classificationCode));
        }

        return trimmed;
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

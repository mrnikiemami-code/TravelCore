using NodaTime;

namespace TravelCore.Modules.Content.Domain;

/// <summary>
/// Content editorial aggregate root (P08-R1).
/// Shared editorial facts live here; type-specific facts live on Article/LandingPage/Guide rows (1:1).
/// Localized title/body/excerpt live on ContentItemTranslation rows (T003; ADR 0008).
/// Blocks / slug / SEO / Destination / Author / Media / delete-archive are later P08 tasks (R2–R8).
/// </summary>
public sealed class ContentItem
{
    public const int CodeMaxLength = 64;
    public const int NameMaxLength = 200;

    private readonly List<ContentItemTranslation> _translations = [];

    private ContentItem()
    {
        Code = null!;
        EnglishName = null!;
    }

    private ContentItem(
        ContentItemId id,
        ContentKind kind,
        string code,
        string englishName,
        Instant createdAt,
        Article? article,
        LandingPage? landingPage,
        Guide? guide)
    {
        Id = id;
        Kind = kind;
        Code = code;
        EnglishName = englishName;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        Article = article;
        LandingPage = landingPage;
        Guide = guide;
        ValidateSpecializationInvariant(id, kind, article, landingPage, guide);
    }

    public ContentItemId Id { get; private set; }

    public ContentKind Kind { get; private set; }

    /// <summary>Stable opaque content code within TravelCore (not SEO slug).</summary>
    public string Code { get; private set; }

    /// <summary>Baseline English display name (localized titles live in translations).</summary>
    public string EnglishName { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public Article? Article { get; private set; }

    public LandingPage? LandingPage { get; private set; }

    public Guide? Guide { get; private set; }

    public IReadOnlyCollection<ContentItemTranslation> Translations => _translations;

    public static ContentItem CreateArticle(
        string code,
        string englishName,
        Instant now,
        ContentItemId? id = null)
    {
        var contentItemId = id ?? ContentItemId.New();
        return new ContentItem(
            contentItemId,
            ContentKind.Article,
            NormalizeCode(code),
            NormalizeName(englishName),
            now,
            article: Domain.Article.Create(contentItemId),
            landingPage: null,
            guide: null);
    }

    public static ContentItem CreateLandingPage(
        string code,
        string englishName,
        Instant now,
        ContentItemId? id = null)
    {
        var contentItemId = id ?? ContentItemId.New();
        return new ContentItem(
            contentItemId,
            ContentKind.LandingPage,
            NormalizeCode(code),
            NormalizeName(englishName),
            now,
            article: null,
            landingPage: Domain.LandingPage.Create(contentItemId),
            guide: null);
    }

    public static ContentItem CreateGuide(
        string code,
        string englishName,
        Instant now,
        ContentItemId? id = null)
    {
        var contentItemId = id ?? ContentItemId.New();
        return new ContentItem(
            contentItemId,
            ContentKind.Guide,
            NormalizeCode(code),
            NormalizeName(englishName),
            now,
            article: null,
            landingPage: null,
            guide: Domain.Guide.Create(contentItemId));
    }

    /// <summary>
    /// Reconstitute a ContentItem with explicit specializations (tests / guarded composition).
    /// </summary>
    public static ContentItem Reconstitute(
        ContentItemId id,
        ContentKind kind,
        string code,
        string englishName,
        Instant createdAt,
        Instant updatedAt,
        Article? article,
        LandingPage? landingPage,
        Guide? guide)
    {
        var item = new ContentItem(
            id,
            kind,
            NormalizeCode(code),
            NormalizeName(englishName),
            createdAt,
            article,
            landingPage,
            guide)
        {
            UpdatedAt = updatedAt
        };
        return item;
    }

    public ContentItemTranslation UpsertTranslation(
        string localeCode,
        string title,
        string? body,
        string? excerpt,
        Instant now)
    {
        var normalizedLocale = ContentItemTranslation.NormalizeLocaleCode(localeCode);
        var existing = _translations.FirstOrDefault(x =>
            string.Equals(x.LocaleCode, normalizedLocale, StringComparison.Ordinal));

        if (existing is null)
        {
            var created = ContentItemTranslation.Create(
                Id,
                normalizedLocale,
                title,
                body,
                excerpt,
                now);
            _translations.Add(created);
            UpdatedAt = now;
            return created;
        }

        existing.Update(title, body, excerpt, now);
        UpdatedAt = now;
        return existing;
    }

    public ContentItemTranslation? FindTranslation(string localeCode)
    {
        var normalizedLocale = ContentItemTranslation.NormalizeLocaleCode(localeCode);
        return _translations.FirstOrDefault(x =>
            string.Equals(x.LocaleCode, normalizedLocale, StringComparison.Ordinal));
    }

    /// <summary>
    /// Kind must match exactly one specialization row; multi-kind and mismatch are rejected.
    /// </summary>
    public static void ValidateSpecializationInvariant(
        ContentItemId contentItemId,
        ContentKind kind,
        Article? article,
        LandingPage? landingPage,
        Guide? guide)
    {
        if (contentItemId.Value == Guid.Empty)
        {
            throw new ArgumentException("ContentItemId cannot be empty.", nameof(contentItemId));
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported ContentKind.");
        }

        var specializationCount =
            (article is null ? 0 : 1)
            + (landingPage is null ? 0 : 1)
            + (guide is null ? 0 : 1);

        if (specializationCount > 1)
        {
            throw new ArgumentException(
                "A ContentItem may have only one typed specialization (Article, LandingPage, or Guide).",
                nameof(kind));
        }

        if (specializationCount == 0)
        {
            throw new ArgumentException(
                "A ContentItem requires exactly one typed specialization matching its ContentKind.",
                nameof(kind));
        }

        switch (kind)
        {
            case ContentKind.Article:
                if (article is null)
                {
                    throw new ArgumentException(
                        "ContentKind.Article requires an Article specialization.",
                        nameof(article));
                }

                if (landingPage is not null || guide is not null)
                {
                    throw new ArgumentException(
                        "ContentKind.Article cannot carry LandingPage or Guide specializations.",
                        nameof(kind));
                }

                EnsureSameContentItemId(contentItemId, article.ContentItemId, nameof(article));
                break;

            case ContentKind.LandingPage:
                if (landingPage is null)
                {
                    throw new ArgumentException(
                        "ContentKind.LandingPage requires a LandingPage specialization.",
                        nameof(landingPage));
                }

                if (article is not null || guide is not null)
                {
                    throw new ArgumentException(
                        "ContentKind.LandingPage cannot carry Article or Guide specializations.",
                        nameof(kind));
                }

                EnsureSameContentItemId(contentItemId, landingPage.ContentItemId, nameof(landingPage));
                break;

            case ContentKind.Guide:
                if (guide is null)
                {
                    throw new ArgumentException(
                        "ContentKind.Guide requires a Guide specialization.",
                        nameof(guide));
                }

                if (article is not null || landingPage is not null)
                {
                    throw new ArgumentException(
                        "ContentKind.Guide cannot carry Article or LandingPage specializations.",
                        nameof(kind));
                }

                EnsureSameContentItemId(contentItemId, guide.ContentItemId, nameof(guide));
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported ContentKind.");
        }
    }

    public static string NormalizeCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        var trimmed = code.Trim();
        if (trimmed.Length > CodeMaxLength)
        {
            throw new ArgumentException($"Content code max length is {CodeMaxLength}.", nameof(code));
        }

        return trimmed;
    }

    public static string NormalizeName(string englishName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(englishName);
        var trimmed = englishName.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"Content name max length is {NameMaxLength}.", nameof(englishName));
        }

        return trimmed;
    }

    private static void EnsureSameContentItemId(ContentItemId expected, ContentItemId actual, string paramName)
    {
        if (expected != actual)
        {
            throw new ArgumentException("Specialization ContentItemId mismatch.", paramName);
        }
    }
}

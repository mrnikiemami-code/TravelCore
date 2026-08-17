using NodaTime;

namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// User-authored travel narrative (TC-P16-T004 / P16-R4). Independent UGC aggregate.
/// Travelogue != ContentItem. No editorial CMS, moderation, or peer FK.
/// </summary>
public sealed class Travelogue
{
    public const int TitleMaxLength = 200;
    public const int BodyMaxLength = 8000;
    public const int LocaleCodeMaxLength = 16;

    private Travelogue()
    {
        Title = null!;
        Body = null!;
        LocaleCode = null!;
    }

    private Travelogue(
        TravelogueId id,
        Guid actorId,
        string localeCode,
        string title,
        string body,
        Instant createdAt)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("TravelogueId cannot be empty.", nameof(id));
        }

        if (actorId == Guid.Empty)
        {
            throw new ArgumentException("ActorId cannot be empty.", nameof(actorId));
        }

        Id = id;
        ActorId = actorId;
        LocaleCode = NormalizeLocaleCode(localeCode);
        Title = NormalizeRequired(title, TitleMaxLength, nameof(title));
        Body = NormalizeRequired(body, BodyMaxLength, nameof(body));
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public TravelogueId Id { get; private set; }

    /// <summary>Opaque logical actor id. Not Identity/Party ownership.</summary>
    public Guid ActorId { get; private set; }

    public string LocaleCode { get; private set; }

    public string Title { get; private set; }

    public string Body { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public static Travelogue Create(
        Guid actorId,
        string localeCode,
        string title,
        string body,
        Instant now) =>
        new(TravelogueId.New(), actorId, localeCode, title, body, now);

    public void SetText(string title, string body, Instant now)
    {
        Title = NormalizeRequired(title, TitleMaxLength, nameof(title));
        Body = NormalizeRequired(body, BodyMaxLength, nameof(body));
        Touch(now);
    }

    public void SetLocale(string localeCode, Instant now)
    {
        LocaleCode = NormalizeLocaleCode(localeCode);
        Touch(now);
    }

    private void Touch(Instant now) => UpdatedAt = now;

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

    private static string NormalizeRequired(string value, int maxLength, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} cannot exceed {maxLength} characters.", paramName);
        }

        return trimmed;
    }
}

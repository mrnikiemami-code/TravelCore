using NodaTime;

namespace TravelCore.Modules.Content.Domain;

/// <summary>
/// Content-owned editorial Tag catalog entry (TC-P08-T004).
/// Not ReferenceData · not Party · not Author (P08-R7 still open).
/// </summary>
public sealed class ContentTag
{
    public const int CodeMaxLength = 64;
    public const int NameMaxLength = 200;

    private ContentTag()
    {
        Code = null!;
        EnglishName = null!;
    }

    private ContentTag(
        ContentTagId id,
        string code,
        string englishName,
        Instant createdAt)
    {
        Id = id;
        Code = code;
        EnglishName = englishName;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public ContentTagId Id { get; private set; }

    public string Code { get; private set; }

    public string EnglishName { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public static ContentTag Create(
        string code,
        string englishName,
        Instant now,
        ContentTagId? id = null)
    {
        return new ContentTag(
            id ?? ContentTagId.New(),
            NormalizeCode(code),
            NormalizeName(englishName),
            now);
    }

    public void Rename(string englishName, Instant now)
    {
        EnglishName = NormalizeName(englishName);
        UpdatedAt = now;
    }

    public static string NormalizeCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        var trimmed = code.Trim();
        if (trimmed.Length > CodeMaxLength)
        {
            throw new ArgumentException($"Tag code max length is {CodeMaxLength}.", nameof(code));
        }

        return trimmed;
    }

    public static string NormalizeName(string englishName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(englishName);
        var trimmed = englishName.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"Tag name max length is {NameMaxLength}.", nameof(englishName));
        }

        return trimmed;
    }
}

using NodaTime;

namespace TravelCore.Modules.Content.Domain;

/// <summary>
/// Content-owned editorial Category catalog entry (TC-P08-T004).
/// Not ReferenceData · not Party · not a dumping-ground CMS folder tree.
/// </summary>
public sealed class ContentCategory
{
    public const int CodeMaxLength = 64;
    public const int NameMaxLength = 200;

    private ContentCategory()
    {
        Code = null!;
        EnglishName = null!;
    }

    private ContentCategory(
        ContentCategoryId id,
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

    public ContentCategoryId Id { get; private set; }

    public string Code { get; private set; }

    public string EnglishName { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public static ContentCategory Create(
        string code,
        string englishName,
        Instant now,
        ContentCategoryId? id = null)
    {
        return new ContentCategory(
            id ?? ContentCategoryId.New(),
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
            throw new ArgumentException($"Category code max length is {CodeMaxLength}.", nameof(code));
        }

        return trimmed;
    }

    public static string NormalizeName(string englishName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(englishName);
        var trimmed = englishName.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"Category name max length is {NameMaxLength}.", nameof(englishName));
        }

        return trimmed;
    }
}

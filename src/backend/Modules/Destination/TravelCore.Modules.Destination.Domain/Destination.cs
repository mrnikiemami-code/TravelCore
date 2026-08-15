using NodaTime;

namespace TravelCore.Modules.Destination.Domain;

/// <summary>
/// Destination hierarchy node. Owns travel discovery geography — not Place/Tour/Content.
/// </summary>
public sealed class Destination
{
    public const int CodeMaxLength = 64;
    public const int NameMaxLength = 200;

    private Destination()
    {
        Code = null!;
        EnglishName = null!;
    }

    private Destination(
        DestinationId id,
        DestinationKind kind,
        string code,
        string englishName,
        DestinationId? parentId,
        string? isoCountryCode,
        Instant createdAt)
    {
        Id = id;
        Kind = kind;
        Code = code;
        EnglishName = englishName;
        ParentId = parentId;
        IsoCountryCode = isoCountryCode;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public DestinationId Id { get; private set; }

    public DestinationKind Kind { get; private set; }

    /// <summary>Stable opaque destination code within TravelCore (not SEO slug — that is T006).</summary>
    public string Code { get; private set; }

    /// <summary>Baseline English display name until T004 translations land.</summary>
    public string EnglishName { get; private set; }

    public DestinationId? ParentId { get; private set; }

    /// <summary>
    /// Optional ReferenceData ISO alpha-2 country code. Required when <see cref="Kind"/> is Country.
    /// Does not replace ReferenceData country catalog ownership.
    /// </summary>
    public string? IsoCountryCode { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public static Destination Create(
        DestinationKind kind,
        string code,
        string englishName,
        Instant now,
        DestinationId? parentId = null,
        string? isoCountryCode = null,
        Destination? parent = null,
        DestinationId? id = null)
    {
        var normalizedCode = NormalizeCode(code);
        var normalizedName = NormalizeName(englishName);
        var normalizedIso = NormalizeOptionalIsoCountry(isoCountryCode);

        ValidateHierarchy(kind, parentId, parent, normalizedIso);

        return new Destination(
            id ?? DestinationId.New(),
            kind,
            normalizedCode,
            normalizedName,
            parentId,
            normalizedIso,
            now);
    }

    public static void ValidateHierarchy(
        DestinationKind kind,
        DestinationId? parentId,
        Destination? parent,
        string? isoCountryCode)
    {
        switch (kind)
        {
            case DestinationKind.Country:
                if (parentId is not null || parent is not null)
                {
                    throw new ArgumentException("Country destinations must not have a parent.", nameof(parentId));
                }

                if (string.IsNullOrWhiteSpace(isoCountryCode))
                {
                    throw new ArgumentException("Country destinations require IsoCountryCode (ReferenceData alpha-2).", nameof(isoCountryCode));
                }

                break;

            case DestinationKind.Region:
                RequireParentKind(parentId, parent, allowed: [DestinationKind.Country], kindName: "Region");
                break;

            case DestinationKind.City:
                RequireParentKind(parentId, parent, allowed: [DestinationKind.Country, DestinationKind.Region], kindName: "City");
                break;

            case DestinationKind.Area:
                RequireParentKind(parentId, parent, allowed: [DestinationKind.Region, DestinationKind.City], kindName: "Area");
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported DestinationKind.");
        }

        if (kind != DestinationKind.Country && !string.IsNullOrWhiteSpace(isoCountryCode))
        {
            throw new ArgumentException("Only Country destinations may set IsoCountryCode.", nameof(isoCountryCode));
        }
    }

    private static void RequireParentKind(
        DestinationId? parentId,
        Destination? parent,
        DestinationKind[] allowed,
        string kindName)
    {
        if (parentId is null)
        {
            throw new ArgumentException($"{kindName} destinations require a parent.", nameof(parentId));
        }

        if (parent is null)
        {
            // Parent existence is validated by application service; kind check when parent loaded.
            return;
        }

        if (parent.Id != parentId.Value)
        {
            throw new ArgumentException("Parent entity id does not match parentId.", nameof(parent));
        }

        if (!allowed.Contains(parent.Kind))
        {
            throw new ArgumentException(
                $"{kindName} parent must be one of: {string.Join(", ", allowed)}.",
                nameof(parent));
        }
    }

    private static string NormalizeCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        var trimmed = code.Trim();
        if (trimmed.Length > CodeMaxLength)
        {
            throw new ArgumentException($"Destination code max length is {CodeMaxLength}.", nameof(code));
        }

        return trimmed;
    }

    private static string NormalizeName(string englishName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(englishName);
        var trimmed = englishName.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"Destination name max length is {NameMaxLength}.", nameof(englishName));
        }

        return trimmed;
    }

    private static string? NormalizeOptionalIsoCountry(string? isoCountryCode)
    {
        if (string.IsNullOrWhiteSpace(isoCountryCode))
        {
            return null;
        }

        var trimmed = isoCountryCode.Trim().ToUpperInvariant();
        if (trimmed.Length != 2 || !trimmed.All(static c => c is >= 'A' and <= 'Z'))
        {
            throw new ArgumentException("IsoCountryCode must be ISO 3166-1 alpha-2.", nameof(isoCountryCode));
        }

        return trimmed;
    }
}

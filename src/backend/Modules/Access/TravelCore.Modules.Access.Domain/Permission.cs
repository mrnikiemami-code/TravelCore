using NodaTime;

namespace TravelCore.Modules.Access.Domain;

/// <summary>
/// Access-owned permission catalog entry. Not a UI visibility flag.
/// </summary>
public sealed class Permission
{
    public const int CodeMaxLength = 128;
    public const int NameMaxLength = 200;

    private Permission()
    {
        Code = null!;
        DisplayName = null!;
    }

    private Permission(PermissionId id, string code, string displayName, Instant createdAt)
    {
        Id = id;
        Code = NormalizeCode(code);
        DisplayName = NormalizeName(displayName);
        CreatedAt = createdAt;
    }

    public PermissionId Id { get; private set; }

    public string Code { get; private set; }

    public string DisplayName { get; private set; }

    public Instant CreatedAt { get; private set; }

    public static Permission Create(string code, string displayName, Instant now, PermissionId? id = null)
        => new(id ?? PermissionId.New(), code, displayName, now);

    private static string NormalizeCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        var trimmed = code.Trim().ToLowerInvariant();
        if (trimmed.Length > CodeMaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(code));
        }

        return trimmed;
    }

    private static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var trimmed = name.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(name));
        }

        return trimmed;
    }
}

using NodaTime;

namespace TravelCore.Modules.Access.Domain;

/// <summary>
/// Access-owned role. Does not own credentials or Party profiles.
/// </summary>
public sealed class Role
{
    public const int CodeMaxLength = 64;
    public const int NameMaxLength = 200;

    private readonly List<RolePermission> _permissions = [];

    private Role()
    {
        Code = null!;
        DisplayName = null!;
    }

    private Role(RoleId id, string code, string displayName, Instant createdAt)
    {
        Id = id;
        Code = NormalizeCode(code);
        DisplayName = NormalizeName(displayName);
        CreatedAt = createdAt;
    }

    public RoleId Id { get; private set; }

    public string Code { get; private set; }

    public string DisplayName { get; private set; }

    public Instant CreatedAt { get; private set; }

    public IReadOnlyCollection<RolePermission> Permissions => _permissions;

    public static Role Create(string code, string displayName, Instant now, RoleId? id = null)
        => new(id ?? RoleId.New(), code, displayName, now);

    public void GrantPermission(PermissionId permissionId, Instant now)
    {
        if (_permissions.Any(x => x.PermissionId.Equals(permissionId)))
        {
            return;
        }

        _permissions.Add(new RolePermission(Id, permissionId, now));
    }

    public void RevokePermission(PermissionId permissionId)
    {
        _permissions.RemoveAll(x => x.PermissionId.Equals(permissionId));
    }

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

/// <summary>
/// Role↔Permission membership inside Access schema only.
/// </summary>
public sealed class RolePermission
{
    private RolePermission()
    {
    }

    internal RolePermission(RoleId roleId, PermissionId permissionId, Instant grantedAt)
    {
        RoleId = roleId;
        PermissionId = permissionId;
        GrantedAt = grantedAt;
    }

    public RoleId RoleId { get; private set; }

    public PermissionId PermissionId { get; private set; }

    public Instant GrantedAt { get; private set; }
}

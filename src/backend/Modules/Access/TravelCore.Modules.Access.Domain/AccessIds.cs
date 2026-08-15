using TravelCore.Identifiers;

namespace TravelCore.Modules.Access.Domain;

public readonly record struct PermissionId(Guid Value)
{
    public static PermissionId New() => new(Uuid7.New());

    public static PermissionId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("PermissionId cannot be empty.", nameof(value));
        }

        return new PermissionId(value);
    }
}

public readonly record struct RoleId(Guid Value)
{
    public static RoleId New() => new(Uuid7.New());

    public static RoleId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("RoleId cannot be empty.", nameof(value));
        }

        return new RoleId(value);
    }
}

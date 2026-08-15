using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace TravelCore.Modules.Access.Contracts;

public sealed class CreatePermissionRequest
{
    [Required]
    [MaxLength(128)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;
}

public sealed class PermissionResponse
{
    public required Guid Id { get; init; }

    public required string Code { get; init; }

    public required string DisplayName { get; init; }

    public required Instant CreatedAt { get; init; }
}

public sealed class CreateRoleRequest
{
    [Required]
    [MaxLength(64)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;
}

public sealed class RoleResponse
{
    public required Guid Id { get; init; }

    public required string Code { get; init; }

    public required string DisplayName { get; init; }

    public required Instant CreatedAt { get; init; }

    public required IReadOnlyList<Guid> PermissionIds { get; init; }
}

public sealed class GrantRolePermissionRequest
{
    [Required]
    public Guid PermissionId { get; set; }
}

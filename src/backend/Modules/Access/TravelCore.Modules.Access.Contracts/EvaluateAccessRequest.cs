using System.ComponentModel.DataAnnotations;

namespace TravelCore.Modules.Access.Contracts;

/// <summary>
/// Probe request for Access evaluation (server authority). Deny-by-default.
/// Subject assignments arrive in T007; until then RoleIds may be supplied for taxonomy-backed evaluation.
/// </summary>
public sealed class EvaluateAccessRequest
{
    /// <summary>
    /// Optional: Identity | Party (conventions from T002/T003). Assignment lookup deferred to T007.
    /// </summary>
    [MaxLength(32)]
    public string? SubjectType { get; set; }

    public Guid? SubjectId { get; set; }

    [Required]
    [MaxLength(128)]
    public string PermissionCode { get; set; } = string.Empty;

    /// <summary>
    /// Temporary evaluation context until subject→role assignment (T007) exists.
    /// </summary>
    public List<Guid>? RoleIds { get; set; }
}

public sealed class EvaluateAccessResponse
{
    public required bool Allowed { get; init; }

    public required string PermissionCode { get; init; }

    public required string Decision { get; init; }

    public string? Reason { get; init; }
}

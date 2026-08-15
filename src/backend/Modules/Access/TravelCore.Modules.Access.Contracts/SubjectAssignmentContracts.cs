using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace TravelCore.Modules.Access.Contracts;

public sealed class AssignSubjectRoleRequest
{
    /// <summary>
    /// Identity | Party
    /// </summary>
    [Required]
    [MaxLength(32)]
    public string SubjectType { get; set; } = string.Empty;

    [Required]
    public Guid SubjectId { get; set; }

    [Required]
    public Guid RoleId { get; set; }
}

public sealed class SubjectRoleAssignmentResponse
{
    public required string SubjectType { get; init; }

    public required Guid SubjectId { get; init; }

    public required Guid RoleId { get; init; }

    public required Instant AssignedAt { get; init; }
}

using NodaTime;

namespace TravelCore.Modules.Access.Domain;

/// <summary>
/// Access-owned subject→role assignment. Subject ids are opaque (no cross-schema FK).
/// </summary>
public sealed class SubjectRoleAssignment
{
    private SubjectRoleAssignment()
    {
    }

    public SubjectRoleAssignment(
        AccessSubjectKind subjectKind,
        Guid subjectId,
        RoleId roleId,
        Instant assignedAt)
    {
        if (subjectId == Guid.Empty)
        {
            throw new ArgumentException("Subject id cannot be empty.", nameof(subjectId));
        }

        SubjectKind = subjectKind;
        SubjectId = subjectId;
        RoleId = roleId;
        AssignedAt = assignedAt;
    }

    public AccessSubjectKind SubjectKind { get; private set; }

    public Guid SubjectId { get; private set; }

    public RoleId RoleId { get; private set; }

    public Instant AssignedAt { get; private set; }
}

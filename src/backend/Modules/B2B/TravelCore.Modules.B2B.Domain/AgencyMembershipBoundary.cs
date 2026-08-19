namespace TravelCore.Modules.B2B.Domain;

/// <summary>
/// P24-R2: Agency membership posture — user linkage is Access-owned; B2B may reference subjects logically only.
/// </summary>
public sealed class AgencyMembershipBoundary
{
    private AgencyMembershipBoundary()
    {
        Agency = default!;
    }

    private AgencyMembershipBoundary(AgencyReference agency, AccessSubjectReferenceId accessSubjectId)
    {
        ArgumentNullException.ThrowIfNull(agency);
        if (accessSubjectId.Value == Guid.Empty)
        {
            throw new ArgumentException("Access subject reference cannot be empty.", nameof(accessSubjectId));
        }

        Agency = agency;
        AccessSubjectId = accessSubjectId;
    }

    public AgencyReference Agency { get; private set; }

    /// <summary>
    /// Logical Access subject for an agency user. Authorization data remains in Access module.
    /// </summary>
    public AccessSubjectReferenceId AccessSubjectId { get; private set; }

    public static AgencyMembershipBoundary DescribeMembership(
        AgencyReference agency,
        AccessSubjectReferenceId accessSubjectId) =>
        new(agency, accessSubjectId);
}

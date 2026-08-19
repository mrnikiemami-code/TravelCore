namespace TravelCore.Modules.B2B.Domain;

/// <summary>
/// P24-R3: Logical membership intent/reference only. B2B does not store users, credentials, or authorization data.
/// </summary>
public sealed class AgencyMemberReference
{
    private AgencyMemberReference()
    {
        Agency = default!;
    }

    private AgencyMemberReference(AgencyReference agency, AccessSubjectReferenceId accessSubjectId)
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
    /// Logical Access subject reference for the agency member. Authorization remains Access-owned.
    /// </summary>
    public AccessSubjectReferenceId AccessSubjectId { get; private set; }

    public static AgencyMemberReference DescribeIntent(
        AgencyReference agency,
        AccessSubjectReferenceId accessSubjectId) =>
        new(agency, accessSubjectId);
}

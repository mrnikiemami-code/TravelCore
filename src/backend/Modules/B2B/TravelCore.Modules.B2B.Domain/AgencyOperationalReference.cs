namespace TravelCore.Modules.B2B.Domain;

/// <summary>
/// Logical reference linking agency operational posture without introducing mutations or APIs.
/// </summary>
public sealed class AgencyOperationalReference
{
    private AgencyOperationalReference()
    {
        Agency = default!;
        Reporting = default!;
        Capability = default!;
    }

    private AgencyOperationalReference(
        AgencyReference agency,
        AgencyReportingReference reporting,
        AgencyOperationalCapabilityReference capability)
    {
        ArgumentNullException.ThrowIfNull(agency);
        ArgumentNullException.ThrowIfNull(reporting);
        ArgumentNullException.ThrowIfNull(capability);
        Agency = agency;
        Reporting = reporting;
        Capability = capability;
    }

    public AgencyReference Agency { get; private set; }
    public AgencyReportingReference Reporting { get; private set; }
    public AgencyOperationalCapabilityReference Capability { get; private set; }

    public static AgencyOperationalReference DescribeOperationalIntent(
        AgencyReference agency,
        AgencyReportingReference reporting,
        AgencyOperationalCapabilityReference capability) =>
        new(agency, reporting, capability);
}

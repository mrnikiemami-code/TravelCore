namespace TravelCore.Modules.B2B.Domain;

/// <summary>
/// P24-R4: Logical commercial profile posture for a Party-owned agency in B2B. No financial execution or persistence.
/// </summary>
public sealed class AgencyBusinessReference
{
    private AgencyBusinessReference()
    {
        Agency = default!;
        Capability = default!;
    }

    private AgencyBusinessReference(AgencyReference agency, CommercialCapabilityReference capability)
    {
        ArgumentNullException.ThrowIfNull(agency);
        ArgumentNullException.ThrowIfNull(capability);
        Agency = agency;
        Capability = capability;
    }

    public AgencyReference Agency { get; private set; }

    /// <summary>
    /// Declared commercial capability intent only. Execution remains in Booking/Payment/Pricing owners.
    /// </summary>
    public CommercialCapabilityReference Capability { get; private set; }

    public static AgencyBusinessReference DescribeCommercialIntent(
        AgencyReference agency,
        CommercialCapabilityReference capability) =>
        new(agency, capability);
}

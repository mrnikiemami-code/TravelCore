namespace TravelCore.Modules.B2B.Domain;

/// <summary>
/// Logical distribution intent linking agency commercial posture to a sales channel reference.
/// </summary>
public sealed class AgencyDistributionReference
{
    private AgencyDistributionReference()
    {
        Agency = default!;
        Channel = default!;
        Capability = default!;
    }

    private AgencyDistributionReference(
        AgencyReference agency,
        SalesChannelReference channel,
        DistributionCapabilityReference capability)
    {
        ArgumentNullException.ThrowIfNull(agency);
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(capability);
        Agency = agency;
        Channel = channel;
        Capability = capability;
    }

    public AgencyReference Agency { get; private set; }

    public SalesChannelReference Channel { get; private set; }

    public DistributionCapabilityReference Capability { get; private set; }

    public static AgencyDistributionReference DescribeDistributionIntent(
        AgencyReference agency,
        SalesChannelReference channel,
        DistributionCapabilityReference capability) =>
        new(agency, channel, capability);
}

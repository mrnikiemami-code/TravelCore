namespace TravelCore.Modules.B2B.Domain;

/// <summary>
/// Logical payment relationship intent for agency commerce.
/// </summary>
public sealed class AgencyPaymentReference
{
    private AgencyPaymentReference()
    {
        Agency = default!;
        Responsibility = default!;
        Capability = default!;
    }

    private AgencyPaymentReference(
        AgencyReference agency,
        PaymentResponsibilityReference responsibility,
        CommercialPaymentCapabilityReference capability)
    {
        ArgumentNullException.ThrowIfNull(agency);
        ArgumentNullException.ThrowIfNull(responsibility);
        ArgumentNullException.ThrowIfNull(capability);

        Agency = agency;
        Responsibility = responsibility;
        Capability = capability;
    }

    public AgencyReference Agency { get; private set; }
    public PaymentResponsibilityReference Responsibility { get; private set; }
    public CommercialPaymentCapabilityReference Capability { get; private set; }

    public static AgencyPaymentReference DescribeRelationship(
        AgencyReference agency,
        PaymentResponsibilityReference responsibility,
        CommercialPaymentCapabilityReference capability) =>
        new(agency, responsibility, capability);
}

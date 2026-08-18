namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Opaque logical reference to a Pricing-owned Quote. Not an EF FK into schema pricing.
/// </summary>
public readonly record struct PricingQuoteReference(Guid LogicalId)
{
    public static PricingQuoteReference From(Guid logicalId)
    {
        if (logicalId == Guid.Empty)
        {
            throw new ArgumentException("PricingQuoteReference cannot be empty.", nameof(logicalId));
        }

        return new PricingQuoteReference(logicalId);
    }

    public override string ToString() => LogicalId.ToString("D");
}

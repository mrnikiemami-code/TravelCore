namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Opaque logical AgencyOffer id. Not an AgencyOffer clone and not a Quote (P19-R7).
/// </summary>
public readonly record struct AgencyOfferReference(Guid AgencyOfferId)
{
    public AgencyOfferReference()
        : this(Guid.Empty)
    {
    }
}

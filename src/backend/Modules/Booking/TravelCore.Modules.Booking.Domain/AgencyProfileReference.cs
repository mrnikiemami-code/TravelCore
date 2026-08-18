namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Opaque logical AgencyProfile id. Not an AgencyProfile clone (P19-R7).
/// </summary>
public readonly record struct AgencyProfileReference(Guid AgencyProfileId)
{
    public AgencyProfileReference()
        : this(Guid.Empty)
    {
    }
}

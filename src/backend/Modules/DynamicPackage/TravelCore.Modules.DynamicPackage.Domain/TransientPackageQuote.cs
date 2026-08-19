namespace TravelCore.Modules.DynamicPackage.Domain;

/// <summary>
/// Transient (non-persistent) package quote combining a composition candidate
/// with its monetary snapshot. Not an aggregate — no lifecycle, no booking identity.
/// DynamicPackage is NOT the price authority; component prices remain
/// Flight-owned (FlightOfferSnapshot) and Hotel-owned (HotelRateOfferSnapshot).
/// Discount/markup/commission: DEFERRED — no evidence in repository.
/// </summary>
public sealed class TransientPackageQuote
{
    private TransientPackageQuote(
        TransientPackageCandidate candidate,
        PackageMonetarySnapshot monetary)
    {
        Candidate = candidate;
        Monetary = monetary;
    }

    public TransientPackageCandidate Candidate { get; }

    public PackageMonetarySnapshot Monetary { get; }

    public static TransientPackageQuote Create(
        TransientPackageCandidate candidate,
        PackageMonetarySnapshot monetary)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(monetary);
        return new TransientPackageQuote(candidate, monetary);
    }
}

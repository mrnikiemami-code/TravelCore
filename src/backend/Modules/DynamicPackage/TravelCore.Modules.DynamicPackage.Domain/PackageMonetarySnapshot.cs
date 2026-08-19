namespace TravelCore.Modules.DynamicPackage.Domain;

/// <summary>
/// Immutable, transient monetary snapshot for a DynamicPackage composition.
/// Aggregates component totals into a single package total.
/// DynamicPackage is NOT the price authority — Flight and Hotel modules own their monetary snapshots.
/// Same-currency enforcement: mixed currencies are rejected (ADR 0003).
/// </summary>
public sealed class PackageMonetarySnapshot
{
    private PackageMonetarySnapshot(
        Money.Money flightTotal,
        Money.Money hotelTotal,
        Money.Money packageTotal)
    {
        FlightTotal = flightTotal;
        HotelTotal = hotelTotal;
        PackageTotal = packageTotal;
    }

    public Money.Money FlightTotal { get; }

    public Money.Money HotelTotal { get; }

    public Money.Money PackageTotal { get; }

    /// <summary>
    /// Creates a package monetary snapshot from component totals.
    /// Both components must use the same currency (mixed-currency rejected per ADR 0003).
    /// </summary>
    public static PackageMonetarySnapshot Create(
        Money.Money flightTotal,
        Money.Money hotelTotal)
    {
        ArgumentNullException.ThrowIfNull(flightTotal);
        ArgumentNullException.ThrowIfNull(hotelTotal);

        if (!flightTotal.Currency.Equals(hotelTotal.Currency))
        {
            throw new InvalidOperationException(
                $"Mixed-currency package rejected: Flight={flightTotal.Currency}, Hotel={hotelTotal.Currency}. " +
                "Same currency required per ADR 0003.");
        }

        var packageTotal = flightTotal.Add(hotelTotal);
        return new PackageMonetarySnapshot(flightTotal, hotelTotal, packageTotal);
    }
}

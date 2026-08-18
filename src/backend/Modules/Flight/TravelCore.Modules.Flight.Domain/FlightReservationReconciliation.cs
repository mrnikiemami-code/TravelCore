using TravelCore.Modules.Flight.Contracts;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Structural/commercial match rules before local FlightSupplierReservation.Confirmed.
/// Mismatch never mutates FlightBooking or accepted offer/monetary snapshots.
/// </summary>
public static class FlightReservationReconciliation
{
    public static bool SegmentsMatch(
        IReadOnlyCollection<FlightOfferSegmentIdentity> confirmed,
        IReadOnlyCollection<FlightOfferSegmentIdentity> expected)
    {
        ArgumentNullException.ThrowIfNull(confirmed);
        ArgumentNullException.ThrowIfNull(expected);
        if (confirmed.Count != expected.Count)
        {
            return false;
        }

        var remaining = confirmed.ToList();
        foreach (var segment in expected)
        {
            var index = remaining.FindIndex(candidate => candidate.Equals(segment));
            if (index < 0)
            {
                return false;
            }

            remaining.RemoveAt(index);
        }

        return remaining.Count == 0;
    }

    public static bool PassengersMatch(
        IReadOnlyCollection<FlightReservationPassengerFact> confirmed,
        IReadOnlyCollection<FlightReservationPassengerFact> expected)
    {
        ArgumentNullException.ThrowIfNull(confirmed);
        ArgumentNullException.ThrowIfNull(expected);
        if (confirmed.Count != expected.Count)
        {
            return false;
        }

        var remaining = confirmed.ToList();
        foreach (var passenger in expected)
        {
            var index = remaining.FindIndex(candidate => candidate.Equals(passenger));
            if (index < 0)
            {
                return false;
            }

            remaining.RemoveAt(index);
        }

        return remaining.Count == 0;
    }

    public static IReadOnlyList<FlightReconciliationIssueKind> CollectIssues(
        IReadOnlyList<FlightOfferSegmentIdentity> expectedSegments,
        IReadOnlyList<FlightReservationPassengerFact> expectedPassengers,
        IReadOnlyList<FlightOfferSegmentIdentity> confirmedSegments,
        IReadOnlyList<FlightReservationPassengerFact> confirmedPassengers,
        string acceptedSourceOfferReference,
        string? reportedSourceOfferReference,
        MoneyValue acceptedTotal,
        MoneyValue? reportedTotal)
    {
        ArgumentNullException.ThrowIfNull(expectedSegments);
        ArgumentNullException.ThrowIfNull(expectedPassengers);
        ArgumentNullException.ThrowIfNull(confirmedSegments);
        ArgumentNullException.ThrowIfNull(confirmedPassengers);
        ArgumentNullException.ThrowIfNull(acceptedTotal);

        var kinds = new List<FlightReconciliationIssueKind>();
        if (!SegmentsMatch(confirmedSegments, expectedSegments))
        {
            kinds.Add(FlightReconciliationIssueKind.ItineraryMismatch);
        }

        if (!PassengersMatch(confirmedPassengers, expectedPassengers))
        {
            kinds.Add(FlightReconciliationIssueKind.PassengerMismatch);
        }

        if (!string.IsNullOrWhiteSpace(reportedSourceOfferReference)
            && !string.Equals(
                reportedSourceOfferReference.Trim(),
                acceptedSourceOfferReference,
                StringComparison.Ordinal))
        {
            kinds.Add(FlightReconciliationIssueKind.OfferMismatch);
        }

        if (reportedTotal is null)
        {
            kinds.Add(FlightReconciliationIssueKind.AmbiguousReservationOutcome);
        }
        else if (reportedTotal.Currency != acceptedTotal.Currency)
        {
            kinds.Add(FlightReconciliationIssueKind.CurrencyMismatch);
        }
        else if (reportedTotal.Amount != acceptedTotal.Amount)
        {
            kinds.Add(FlightReconciliationIssueKind.MonetaryMismatch);
        }

        return kinds;
    }
}

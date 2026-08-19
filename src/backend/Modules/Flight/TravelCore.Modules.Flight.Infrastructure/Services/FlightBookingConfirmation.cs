using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Flight.Domain;
using FlightBookingAggregate = TravelCore.Modules.Flight.Domain.FlightBooking;

namespace TravelCore.Modules.Flight.Infrastructure.Services;

internal static class FlightBookingConfirmation
{
    public static void TryConfirm(
        FlightDbContext db,
        FlightBookingAggregate booking,
        FlightSupplierReservation reservation,
        FlightBookingPaymentEvidence paymentEvidence,
        FlightOfferSnapshot snapshot,
        Instant now)
    {
        if (booking.Status != FlightBookingStatus.Pending)
        {
            return;
        }

        var tickets = db.FlightTickets.Local
            .Where(t => t.FlightBookingId.Equals(booking.Id))
            .ToList();
        if (tickets.Count == 0)
        {
            tickets = db.FlightTickets.Where(t => t.FlightBookingId == booking.Id).ToList();
        }

        var issues = db.FlightReconciliationIssues.Local
            .Where(i => i.FlightBookingId.Equals(booking.Id))
            .ToList();
        if (issues.Count == 0)
        {
            issues = db.FlightReconciliationIssues.Where(i => i.FlightBookingId == booking.Id).ToList();
        }

        try
        {
            booking.ConfirmFromAuthoritativeReservationPaymentAndTickets(
                reservation,
                paymentEvidence,
                tickets,
                snapshot.Monetary,
                issues,
                now);
        }
        catch (InvalidOperationException)
        {
            // Stay Pending; incomplete tickets or blocking reconciliation remain durable.
        }
    }
}

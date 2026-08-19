using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Flight.Infrastructure.Services;

/// <summary>
/// RefundSucceeded is a trigger. R7 confirmed cancellation completes after FullRefund.
/// R6 compensation may Cancel Pending only. Confirmed stays Confirmed unless already Cancelled.
/// PaymentStatus remains Succeeded.
/// </summary>
internal sealed class FlightBookingRefundSucceededIntegrationHandler : IFlightBookingRefundSucceededIntegrationHandler
{
    private readonly FlightDbContext _db;
    private readonly IClock _clock;

    public FlightBookingRefundSucceededIntegrationHandler(FlightDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task HandleAsync(
        FlightBookingRefundSucceededIntegrationEvent message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var existing = await _db.RefundSuccessInbox
            .SingleOrDefaultAsync(x => x.RefundId == message.RefundId, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var now = _clock.GetCurrentInstant();
        var bookingId = FlightBookingId.From(message.FlightBookingId);
        var booking = await _db.FlightBookings
            .Include(x => x.Passengers)
            .SingleOrDefaultAsync(x => x.Id == bookingId, cancellationToken)
            ?? throw new InvalidOperationException("FlightBooking was not found.");

        var cancellation = await _db.FlightBookingCancellations
            .Include(x => x.Attempts)
            .SingleOrDefaultAsync(x => x.FlightBookingId == bookingId, cancellationToken);

        if (cancellation is not null
            && cancellation.PaymentId == message.PaymentId
            && cancellation.RequiresFullRefund)
        {
            if (booking.Status == FlightBookingStatus.Cancelled
                && cancellation.Status is FlightBookingCancellationStatus.RefundPending
                    or FlightBookingCancellationStatus.Completed)
            {
                cancellation.CompleteFromAuthoritativeRefundSuccess(now);
            }
            else if (booking.Status == FlightBookingStatus.Confirmed)
            {
                PersistIssue(
                    booking.Id,
                    FlightReconciliationIssueKind.ContradictorySupplierEvidence,
                    now,
                    "RefundSucceeded cannot cancel a Confirmed FlightBooking.");
            }
            else
            {
                PersistIssue(
                    booking.Id,
                    FlightReconciliationIssueKind.ContradictorySupplierEvidence,
                    now,
                    "RefundSucceeded did not match R7 cancellation completion invariants.");
            }
        }
        else if (booking.Status == FlightBookingStatus.Confirmed)
        {
            PersistIssue(
                booking.Id,
                FlightReconciliationIssueKind.ContradictorySupplierEvidence,
                now,
                "RefundSucceeded cannot cancel a Confirmed FlightBooking.");
        }
        else
        {
            booking.CancelFromAuthoritativePaymentCompensation(now);
        }

        _db.RefundSuccessInbox.Add(FlightRefundSuccessInboxRecord.Create(message.RefundId, now));
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            _db.ChangeTracker.Clear();
        }
    }

    private void PersistIssue(
        FlightBookingId flightBookingId,
        FlightReconciliationIssueKind kind,
        Instant now,
        string detail) =>
        _db.FlightReconciliationIssues.Add(
            new FlightReconciliationIssue(flightBookingId, kind, now, detail: detail));

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException postgres
                && postgres.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return true;
            }
        }

        return false;
    }
}

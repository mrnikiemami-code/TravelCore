using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Flight.Infrastructure.Services;

/// <summary>
/// RefundSucceeded is a trigger. Compensation may Cancel Pending only. Confirmed stays Confirmed (R7).
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
            .SingleOrDefaultAsync(x => x.Id == bookingId, cancellationToken)
            ?? throw new InvalidOperationException("FlightBooking was not found.");

        if (booking.Status == FlightBookingStatus.Confirmed)
        {
            _db.FlightReconciliationIssues.Add(
                new FlightReconciliationIssue(
                    booking.Id,
                    FlightReconciliationIssueKind.ContradictorySupplierEvidence,
                    now,
                    detail: "RefundSucceeded cannot cancel a Confirmed FlightBooking."));
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

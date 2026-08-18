using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Booking.Infrastructure.Services;

/// <summary>
/// Booking consumer of RefundSucceeded. Pending cancels; Confirmed is not cancelled (P20-R6).
/// </summary>
internal sealed class BookingRefundSucceededIntegrationHandler : IRefundSucceededIntegrationHandler
{
    private readonly BookingDbContext _db;
    private readonly BookingCancellationService _cancellation;
    private readonly IClock _clock;

    public BookingRefundSucceededIntegrationHandler(
        BookingDbContext db,
        BookingCancellationService cancellation,
        IClock clock)
    {
        _db = db;
        _cancellation = cancellation;
        _clock = clock;
    }

    public async Task HandleAsync(
        RefundSucceededIntegrationEvent message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var existing = await _db.RefundSuccessInbox
            .SingleOrDefaultAsync(x => x.RefundId == message.RefundId, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var booking = await _db.Bookings
            .SingleOrDefaultAsync(x => x.Id == BookingId.From(message.BookingId), cancellationToken)
            ?? throw new InvalidOperationException("Booking was not found.");

        if (booking.Status == BookingStatus.Confirmed)
        {
            if (!await _db.RefundInvariantIssues.AnyAsync(x => x.RefundId == message.RefundId, cancellationToken))
            {
                _db.RefundInvariantIssues.Add(
                    BookingRefundInvariantIssue.Create(
                        booking.Id,
                        message.RefundId,
                        message.PaymentId,
                        BookingRefundInvariantIssueKind.ConfirmedBooking,
                        _clock.GetCurrentInstant()));
            }
        }
        else
        {
            await _cancellation.CancelPendingAsync(
                booking.Id,
                _clock.GetCurrentInstant(),
                cancellationToken);
        }

        _db.RefundSuccessInbox.Add(
            RefundSuccessInboxRecord.Create(message.RefundId, _clock.GetCurrentInstant()));
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

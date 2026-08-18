using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Booking.Infrastructure.Services;

/// <summary>
/// Booking-owned authoritative payment-success confirmation orchestration (P20-R5 / P20-R6).
/// Payment success is independent of Booking confirmation. Business refusal records
/// BookingConfirmationRecoveryIssue and a compensation-required outbox atomically.
/// </summary>
public sealed class BookingPaymentConfirmationService
{
    private readonly BookingDbContext _db;
    private readonly IPaymentSuccessEvidenceQuery _paymentEvidence;

    public BookingPaymentConfirmationService(
        BookingDbContext db,
        IPaymentSuccessEvidenceQuery paymentEvidence)
    {
        _db = db;
        _paymentEvidence = paymentEvidence;
    }

    public async Task ConfirmIfEligibleAsync(
        BookingId bookingId,
        Instant now,
        CancellationToken cancellationToken = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        await AcquireLockAsync(bookingId.Value, cancellationToken);

        var booking = await _db.Bookings
            .Include(x => x.MonetarySnapshot)
            .Include(x => x.Contact)
            .Include(x => x.Passengers)
            .SingleOrDefaultAsync(x => x.Id == bookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking was not found.");
        var evidence = await _paymentEvidence.GetByBookingIdAsync(bookingId.Value, cancellationToken)
            ?? throw new InvalidOperationException("Payment success evidence was not found.");
        if (!evidence.IsAuthoritativeSuccess)
        {
            throw new InvalidOperationException("Payment is not authoritatively successful.");
        }

        if (booking.Status == BookingStatus.Confirmed)
        {
            await tx.CommitAsync(cancellationToken);
            return;
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            await RecordRecoveryAsync(
                booking.Id,
                evidence.PaymentId,
                BookingConfirmationRecoveryReason.CancelledBooking,
                now,
                cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return;
        }

        if (booking.MonetarySnapshot is null)
        {
            await RecordRecoveryAsync(
                booking.Id,
                evidence.PaymentId,
                BookingConfirmationRecoveryReason.MissingMonetarySnapshot,
                now,
                cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return;
        }

        if (booking.MonetarySnapshot.Total.Amount != evidence.Amount
            || !string.Equals(booking.MonetarySnapshot.Total.Currency.Value, evidence.CurrencyCode, StringComparison.Ordinal))
        {
            await RecordRecoveryAsync(
                booking.Id,
                evidence.PaymentId,
                BookingConfirmationRecoveryReason.MonetaryMismatch,
                now,
                cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return;
        }

        if (booking.Contact is null || booking.PassengerCount == 0)
        {
            await RecordRecoveryAsync(
                booking.Id,
                evidence.PaymentId,
                BookingConfirmationRecoveryReason.MissingPeoplePrerequisites,
                now,
                cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return;
        }

        var hold = await _db.CapacityHolds.SingleOrDefaultAsync(
            x => x.BookingId == bookingId && x.Status == CapacityHoldStatus.Active,
            cancellationToken);
        if (hold is not null && hold.ExpiresAt <= now)
        {
            await AcquireLockAsync(hold.TourDeparture.LogicalId, cancellationToken);
            hold.Expire(now);
            var expiredAccount = await _db.DepartureCapacityAccounts
                .SingleAsync(x => x.TourDeparture == hold.TourDeparture, cancellationToken);
            expiredAccount.ReleaseActive(hold.SeatCount);
            await RecordRecoveryAsync(
                booking.Id,
                evidence.PaymentId,
                BookingConfirmationRecoveryReason.ExpiredHold,
                now,
                cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return;
        }

        if (hold is null)
        {
            var reason = await ResolveMissingActiveHoldReasonAsync(bookingId, cancellationToken);
            await RecordRecoveryAsync(booking.Id, evidence.PaymentId, reason, now, cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return;
        }

        await AcquireLockAsync(hold.TourDeparture.LogicalId, cancellationToken);
        booking.ConfirmFromAuthoritativePaymentSuccess(now);
        hold.Consume(now);
        var account = await _db.DepartureCapacityAccounts
            .SingleAsync(x => x.TourDeparture == hold.TourDeparture, cancellationToken);
        account.ConsumeActive(hold.SeatCount);

        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    private async Task<BookingConfirmationRecoveryReason> ResolveMissingActiveHoldReasonAsync(
        BookingId bookingId,
        CancellationToken cancellationToken)
    {
        var holds = await _db.CapacityHolds
            .Where(x => x.BookingId == bookingId)
            .Select(x => x.Status)
            .ToListAsync(cancellationToken);
        if (holds.Contains(CapacityHoldStatus.Released))
        {
            return BookingConfirmationRecoveryReason.ReleasedHold;
        }

        return BookingConfirmationRecoveryReason.ExpiredHold;
    }

    private async Task RecordRecoveryAsync(
        BookingId bookingId,
        Guid paymentId,
        BookingConfirmationRecoveryReason reason,
        Instant now,
        CancellationToken cancellationToken)
    {
        var existing = await _db.ConfirmationRecoveryIssues
            .SingleOrDefaultAsync(x => x.BookingId == bookingId, cancellationToken);
        if (existing is not null)
        {
            BookingCompensationOutboxWriter.Enqueue(_db, existing, now);
            await _db.SaveChangesAsync(cancellationToken);
            return;
        }

        var issue = BookingConfirmationRecoveryIssue.Create(bookingId, paymentId, reason, now);
        _db.ConfirmationRecoveryIssues.Add(issue);
        BookingCompensationOutboxWriter.Enqueue(_db, issue, now);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private Task AcquireLockAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!_db.Database.IsRelational())
        {
            return Task.CompletedTask;
        }

        var bytes = id.ToByteArray();
        var key1 = BitConverter.ToInt32(bytes, 0);
        var key2 = BitConverter.ToInt32(bytes, 4);
        return _db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({key1}, {key2})",
            cancellationToken);
    }
}

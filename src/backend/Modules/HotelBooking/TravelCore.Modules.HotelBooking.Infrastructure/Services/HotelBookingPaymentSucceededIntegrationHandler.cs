using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.Payment.Contracts;
using Stay = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Services;

/// <summary>
/// Event is a trigger. Confirmation revalidates Payment via query and requires supplier evidence.
/// Does not synchronously complete supplier reservation.
/// </summary>
internal sealed class HotelBookingPaymentSucceededIntegrationHandler : IHotelBookingPaymentSucceededIntegrationHandler
{
    private readonly HotelBookingDbContext _db;
    private readonly IPaymentSuccessEvidenceQuery _paymentEvidence;
    private readonly IClock _clock;

    public HotelBookingPaymentSucceededIntegrationHandler(
        HotelBookingDbContext db,
        IPaymentSuccessEvidenceQuery paymentEvidence,
        IClock clock)
    {
        _db = db;
        _paymentEvidence = paymentEvidence;
        _clock = clock;
    }

    public async Task HandleAsync(
        HotelBookingPaymentSucceededIntegrationEvent message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var existing = await _db.PaymentSuccessInbox
            .SingleOrDefaultAsync(x => x.PaymentId == message.PaymentId, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var now = _clock.GetCurrentInstant();
        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        await AcquireLockAsync(message.HotelBookingId, cancellationToken);

        var bookingId = HotelBookingId.From(message.HotelBookingId);
        var booking = await _db.HotelBookings
            .Include(x => x.Rooms)
            .SingleOrDefaultAsync(x => x.Id == bookingId, cancellationToken)
            ?? throw new InvalidOperationException("HotelBooking was not found.");

        var evidence = await _paymentEvidence.GetByHotelBookingIdAsync(bookingId.Value, cancellationToken)
            ?? throw new InvalidOperationException("Payment success evidence was not found.");
        if (!evidence.IsAuthoritativeSuccess)
        {
            throw new InvalidOperationException("Payment is not authoritatively successful.");
        }

        var snapshot = await _db.HotelRateOfferSnapshots
            .Include(x => x.Monetary)
            .SingleOrDefaultAsync(x => x.HotelBookingId == bookingId, cancellationToken);

        if (snapshot?.Monetary is null
            || evidence.Amount != snapshot.Monetary.Total.Amount
            || !string.Equals(evidence.CurrencyCode, snapshot.Monetary.Total.Currency.Value, StringComparison.Ordinal))
        {
            PersistMismatchIssue(booking.Id, evidence, snapshot, now);
        }
        else
        {
            var local = await _db.HotelBookingPaymentEvidence
                .SingleOrDefaultAsync(x => x.HotelBookingId == bookingId, cancellationToken);
            if (local is null)
            {
                local = HotelBookingPaymentEvidence.Record(
                    bookingId,
                    evidence.PaymentId,
                    evidence.Amount,
                    evidence.CurrencyCode,
                    now);
                _db.HotelBookingPaymentEvidence.Add(local);
            }

            HotelSupplierReservationRequiredOutboxWriter.Enqueue(_db, bookingId, evidence.PaymentId, now);
            await TryConfirmDualEvidenceAsync(booking, snapshot, local, now, cancellationToken);
        }

        _db.PaymentSuccessInbox.Add(
            HotelBookingPaymentSuccessInboxRecord.Create(message.PaymentId, now));
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            await tx.RollbackAsync(cancellationToken);
            _db.ChangeTracker.Clear();
        }
    }

    private async Task TryConfirmDualEvidenceAsync(
        Stay booking,
        HotelRateOfferSnapshot snapshot,
        HotelBookingPaymentEvidence paymentEvidence,
        Instant now,
        CancellationToken cancellationToken)
    {
        if (booking.Status != HotelBookingStatus.Pending)
        {
            return;
        }

        var reservation = await _db.HotelSupplierReservations
            .Include(x => x.Attempts)
            .SingleOrDefaultAsync(x => x.HotelBookingId == booking.Id, cancellationToken);
        if (reservation is null || reservation.Status != HotelSupplierReservationStatus.Confirmed)
        {
            return;
        }

        var existingIssues = await _db.HotelBookingReconciliationIssues
            .Where(x => x.HotelBookingId == booking.Id)
            .ToListAsync(cancellationToken);
        var confirmedRooms = booking.Rooms.Select(r => r.Id).ToArray();

        try
        {
            booking.ConfirmFromAuthoritativePaymentAndSupplierEvidence(
                reservation,
                paymentEvidence,
                now,
                snapshot.Place,
                snapshot.CheckInDate,
                snapshot.CheckOutDate,
                confirmedRooms,
                snapshot.Monetary.Total,
                cancellationTermsMatch: true,
                snapshot.Monetary,
                existingIssues);
        }
        catch (InvalidOperationException)
        {
            // Stay Pending; mismatch/blocking issues already recorded elsewhere.
        }
    }

    private void PersistMismatchIssue(
        HotelBookingId hotelBookingId,
        HotelBookingPaymentSuccessEvidenceRead evidence,
        HotelRateOfferSnapshot? snapshot,
        Instant now)
    {
        var kind = snapshot?.Monetary is null
                || evidence.Amount != snapshot.Monetary.Total.Amount
            ? HotelBookingReconciliationIssueKind.MonetaryMismatch
            : HotelBookingReconciliationIssueKind.CurrencyMismatch;
        _db.HotelBookingReconciliationIssues.Add(
            new HotelBookingReconciliationIssue(hotelBookingId, kind, now, detail: "Payment evidence mismatch."));
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

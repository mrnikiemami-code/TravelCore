using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Payment.Contracts;
using FlightBookingAggregate = TravelCore.Modules.Flight.Domain.FlightBooking;

namespace TravelCore.Modules.Flight.Infrastructure.Services;

/// <summary>
/// Event is a trigger. Flight re-queries Payment evidence and enqueues ticketing asynchronously.
/// Payment callback must not wait for tickets.
/// </summary>
internal sealed class FlightBookingPaymentSucceededIntegrationHandler : IFlightBookingPaymentSucceededIntegrationHandler
{
    private readonly FlightDbContext _db;
    private readonly IPaymentSuccessEvidenceQuery _paymentEvidence;
    private readonly IClock _clock;

    public FlightBookingPaymentSucceededIntegrationHandler(
        FlightDbContext db,
        IPaymentSuccessEvidenceQuery paymentEvidence,
        IClock clock)
    {
        _db = db;
        _paymentEvidence = paymentEvidence;
        _clock = clock;
    }

    public async Task HandleAsync(
        FlightBookingPaymentSucceededIntegrationEvent message,
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
        await AcquireLockAsync(message.FlightBookingId, cancellationToken);

        var bookingId = FlightBookingId.From(message.FlightBookingId);
        var booking = await _db.FlightBookings
            .Include(x => x.Passengers)
            .SingleOrDefaultAsync(x => x.Id == bookingId, cancellationToken)
            ?? throw new InvalidOperationException("FlightBooking was not found.");

        var evidence = await _paymentEvidence.GetByFlightBookingIdAsync(bookingId.Value, cancellationToken)
            ?? throw new InvalidOperationException("Payment success evidence was not found.");
        if (!evidence.IsAuthoritativeSuccess)
        {
            throw new InvalidOperationException("Payment is not authoritatively successful.");
        }

        var snapshot = await _db.FlightOfferSnapshots
            .Include(x => x.Monetary)
            .Include(x => x.FareRules)
            .SingleOrDefaultAsync(x => x.FlightBookingId == bookingId, cancellationToken);

        if (snapshot?.Monetary is null
            || evidence.Amount != snapshot.Monetary.Total.Amount
            || !string.Equals(evidence.CurrencyCode, snapshot.Monetary.Total.Currency.Value, StringComparison.Ordinal))
        {
            _db.FlightReconciliationIssues.Add(
                new FlightReconciliationIssue(
                    booking.Id,
                    FlightReconciliationIssueKind.PaymentEvidenceMismatch,
                    now,
                    detail: "Payment evidence mismatch."));
        }
        else
        {
            var local = await _db.FlightBookingPaymentEvidence
                .SingleOrDefaultAsync(x => x.FlightBookingId == bookingId, cancellationToken);
            if (local is null)
            {
                local = FlightBookingPaymentEvidence.Record(
                    bookingId,
                    evidence.PaymentId,
                    evidence.Amount,
                    evidence.CurrencyCode,
                    now);
                _db.FlightBookingPaymentEvidence.Add(local);
            }

            FlightTicketingRequiredOutboxWriter.Enqueue(_db, bookingId, evidence.PaymentId, now);
            var reservation = await _db.FlightSupplierReservations
                .SingleOrDefaultAsync(x => x.FlightBookingId == bookingId, cancellationToken);
            if (reservation is not null)
            {
                FlightBookingConfirmation.TryConfirm(_db, booking, reservation, local, snapshot, now);
            }
        }

        _db.PaymentSuccessInbox.Add(FlightPaymentSuccessInboxRecord.Create(message.PaymentId, now));
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

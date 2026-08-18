using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Booking.Infrastructure.Services;

/// <summary>
/// Booking-owned authoritative payment-success confirmation orchestration (P20-R5).
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

        if (booking.MonetarySnapshot is null)
        {
            throw new InvalidOperationException("BookingMonetarySnapshot is required.");
        }

        if (booking.MonetarySnapshot.Total.Amount != evidence.Amount
            || !string.Equals(booking.MonetarySnapshot.Total.Currency.Value, evidence.CurrencyCode, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Payment evidence does not match Booking monetary obligation.");
        }

        var hold = await _db.CapacityHolds.SingleOrDefaultAsync(
            x => x.BookingId == bookingId && x.Status == CapacityHoldStatus.Active,
            cancellationToken);
        if (hold is null || hold.ExpiresAt <= now)
        {
            throw new InvalidOperationException("Active, unexpired CapacityHold is required for confirmation.");
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

    private Task AcquireLockAsync(Guid id, CancellationToken cancellationToken)
    {
        var bytes = id.ToByteArray();
        var key1 = BitConverter.ToInt32(bytes, 0);
        var key2 = BitConverter.ToInt32(bytes, 4);
        return _db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({key1}, {key2})",
            cancellationToken);
    }
}

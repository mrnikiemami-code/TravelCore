using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

/// <summary>
/// Trusted preparation that binds Payment to Booking authoritative payable obligation (P20-R5).
/// </summary>
internal sealed class PaymentPreparationService
{
    private readonly PaymentDbContext _db;
    private readonly IBookingPaymentObligationQuery _bookingObligations;
    private readonly IClock _clock;

    public PaymentPreparationService(
        PaymentDbContext db,
        IBookingPaymentObligationQuery bookingObligations,
        IClock clock)
    {
        _db = db;
        _bookingObligations = bookingObligations;
        _clock = clock;
    }

    public async Task PrepareAsync(PaymentId paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _db.Payments
            .Include(x => x.ExecutionSnapshot)
            .SingleOrDefaultAsync(x => x.Id == paymentId, cancellationToken)
            ?? throw new InvalidOperationException("Payment was not found.");

        var obligation = await _bookingObligations.GetByBookingIdAsync(payment.Booking.BookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking payment obligation was not found.");
        if (!obligation.PaymentEligible || !string.Equals(obligation.BookingStatus, "Pending", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Booking is not eligible for Payment preparation.");
        }

        payment.BindExecutionSnapshot(
            obligation.SnapshotId,
            new global::TravelCore.Money.Money(obligation.Amount, obligation.CurrencyCode),
            _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
    }
}

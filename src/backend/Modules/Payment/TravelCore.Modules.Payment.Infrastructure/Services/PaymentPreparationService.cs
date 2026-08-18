using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

/// <summary>
/// Trusted preparation that binds Payment to the target's authoritative payable obligation.
/// Tour uses BookingMonetarySnapshot; Hotel uses HotelBookingMonetarySnapshot (P21-R6).
/// </summary>
internal sealed class PaymentPreparationService
{
    private readonly PaymentDbContext _db;
    private readonly IBookingPaymentObligationQuery _bookingObligations;
    private readonly IHotelBookingPaymentObligationQuery? _hotelObligations;
    private readonly IClock _clock;

    public PaymentPreparationService(
        PaymentDbContext db,
        IBookingPaymentObligationQuery bookingObligations,
        IClock clock,
        IHotelBookingPaymentObligationQuery? hotelObligations = null)
    {
        _db = db;
        _bookingObligations = bookingObligations;
        _clock = clock;
        _hotelObligations = hotelObligations;
    }

    public async Task PrepareAsync(PaymentId paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _db.Payments
            .Include(x => x.ExecutionSnapshot)
            .SingleOrDefaultAsync(x => x.Id == paymentId, cancellationToken)
            ?? throw new InvalidOperationException("Payment was not found.");

        if (payment.HotelBooking is { } hotel)
        {
            var hotelObligation = _hotelObligations is null
                ? throw new InvalidOperationException("HotelBooking payment obligation query is not registered.")
                : await _hotelObligations.GetByHotelBookingIdAsync(hotel.HotelBookingId, cancellationToken)
                    ?? throw new InvalidOperationException("HotelBooking payment obligation was not found.");
            if (!hotelObligation.PaymentEligible
                || !string.Equals(hotelObligation.HotelBookingStatus, "Pending", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("HotelBooking is not eligible for Payment preparation.");
            }

            payment.BindExecutionSnapshot(
                hotelObligation.SnapshotId,
                new global::TravelCore.Money.Money(hotelObligation.Amount, hotelObligation.CurrencyCode),
                _clock.GetCurrentInstant());
            await _db.SaveChangesAsync(cancellationToken);
            return;
        }

        var bookingId = payment.Booking?.BookingId
            ?? throw new InvalidOperationException("Payment has no Tour Booking target.");
        var obligation = await _bookingObligations.GetByBookingIdAsync(bookingId, cancellationToken)
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

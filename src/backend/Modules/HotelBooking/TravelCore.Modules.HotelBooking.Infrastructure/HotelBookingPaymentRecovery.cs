using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure;

internal static class HotelBookingPaymentRecovery
{
    public static async Task<HotelBookingPaymentCompensationEvidence> RecordCompensationAsync(
        HotelBookingDbContext db,
        HotelBookingId hotelBookingId,
        Guid paymentId,
        HotelBookingPaymentCompensationReason reason,
        Instant now,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        var existing = db.PaymentCompensationEvidence.Local
            .SingleOrDefault(x => x.HotelBookingId.Equals(hotelBookingId))
            ?? await db.PaymentCompensationEvidence
                .SingleOrDefaultAsync(x => x.HotelBookingId == hotelBookingId, cancellationToken);
        if (existing is not null)
        {
            HotelBookingCompensationOutboxWriter.Enqueue(db, existing, now);
            return existing;
        }

        var evidence = HotelBookingPaymentCompensationEvidence.Create(
            hotelBookingId,
            paymentId,
            reason,
            now);
        db.PaymentCompensationEvidence.Add(evidence);
        HotelBookingCompensationOutboxWriter.Enqueue(db, evidence, now);
        return evidence;
    }
}

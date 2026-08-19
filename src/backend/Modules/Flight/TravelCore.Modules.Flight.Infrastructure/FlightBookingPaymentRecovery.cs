using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Flight.Infrastructure;

internal static class FlightBookingPaymentRecovery
{
    public static async Task<FlightBookingPaymentCompensationEvidence> RecordCompensationAsync(
        FlightDbContext db,
        FlightBookingId flightBookingId,
        Guid paymentId,
        FlightBookingPaymentCompensationReason reason,
        Instant now,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        var existing = db.PaymentCompensationEvidence.Local
            .SingleOrDefault(x => x.FlightBookingId.Equals(flightBookingId))
            ?? await db.PaymentCompensationEvidence
                .SingleOrDefaultAsync(x => x.FlightBookingId == flightBookingId, cancellationToken);
        if (existing is not null)
        {
            FlightCompensationOutboxWriter.Enqueue(db, existing, now);
            return existing;
        }

        var evidence = FlightBookingPaymentCompensationEvidence.Create(
            flightBookingId,
            paymentId,
            reason,
            now);
        db.PaymentCompensationEvidence.Add(evidence);
        FlightCompensationOutboxWriter.Enqueue(db, evidence, now);
        return evidence;
    }
}

using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Services;

public sealed class HotelSupplierReservationRequiredOutboxDispatcher
{
    private readonly HotelBookingDbContext _db;
    private readonly HotelSupplierReservationService _reservations;
    private readonly IClock _clock;

    public HotelSupplierReservationRequiredOutboxDispatcher(
        HotelBookingDbContext db,
        HotelSupplierReservationService reservations,
        IClock clock)
    {
        _db = db;
        _reservations = reservations;
        _clock = clock;
    }

    public async Task<int> DispatchPendingAsync(int take = 50, CancellationToken cancellationToken = default)
    {
        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take));
        }

        var pending = await _db.OutboxMessages
            .Where(x => x.ProcessedAt == null
                && x.MessageType == HotelSupplierReservationRequiredOutboxBoundary.MessageType)
            .OrderBy(x => x.OccurredAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var dispatched = 0;
        foreach (var message in pending)
        {
            var work = HotelSupplierReservationRequiredOutboxWriter.Deserialize(message.Payload);
            var bookingId = HotelBookingId.From(work.HotelBookingId);
            try
            {
                await _reservations.InitiateAsync(
                    bookingId,
                    idempotencyKey: $"paynow-{work.PaymentId:N}",
                    cancellationToken: cancellationToken);
            }
            catch (InvalidOperationException)
            {
                // Pay-first/hold/unresolved-attempt guards persist evidence; retry later if still pending.
            }

            message.MarkProcessed(_clock.GetCurrentInstant());
            await _db.SaveChangesAsync(cancellationToken);
            dispatched++;
        }

        return dispatched;
    }
}

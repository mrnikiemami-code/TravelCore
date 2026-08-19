using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Services;

public sealed class FlightTicketingRequiredOutboxDispatcher
{
    private readonly FlightDbContext _db;
    private readonly FlightTicketingService _ticketing;
    private readonly IClock _clock;

    public FlightTicketingRequiredOutboxDispatcher(
        FlightDbContext db,
        FlightTicketingService ticketing,
        IClock clock)
    {
        _db = db;
        _ticketing = ticketing;
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
                && x.MessageType == FlightTicketingRequiredOutboxBoundary.MessageType)
            .OrderBy(x => x.OccurredAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var dispatched = 0;
        foreach (var message in pending)
        {
            var work = FlightTicketingRequiredOutboxWriter.Deserialize(message.Payload);
            var bookingId = FlightBookingId.From(work.FlightBookingId);
            try
            {
                await _ticketing.InitiateAsync(
                    bookingId,
                    idempotencyKey: $"paynow-{work.PaymentId:N}",
                    cancellationToken: cancellationToken);
            }
            catch (InvalidOperationException)
            {
                // Guards persist evidence; retry later if still pending.
            }

            message.MarkProcessed(_clock.GetCurrentInstant());
            await _db.SaveChangesAsync(cancellationToken);
            dispatched++;
        }

        return dispatched;
    }
}

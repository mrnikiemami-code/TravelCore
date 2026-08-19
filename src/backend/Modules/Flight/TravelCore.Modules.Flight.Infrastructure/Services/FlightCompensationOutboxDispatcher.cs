using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Flight.Infrastructure.Services;

public sealed class FlightCompensationOutboxDispatcher
{
    private readonly FlightDbContext _db;
    private readonly IFlightBookingPaymentCompensationRequiredHandler _handler;
    private readonly IClock _clock;

    public FlightCompensationOutboxDispatcher(
        FlightDbContext db,
        IFlightBookingPaymentCompensationRequiredHandler handler,
        IClock clock)
    {
        _db = db;
        _handler = handler;
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
                && x.MessageType == FlightBookingCompensationOutboxBoundary.MessageType)
            .OrderBy(x => x.OccurredAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var dispatched = 0;
        foreach (var message in pending)
        {
            var evt = FlightCompensationOutboxSerializer.Deserialize(message.Payload);
            await _handler.HandleAsync(evt, cancellationToken);
            message.MarkProcessed(_clock.GetCurrentInstant());
            await _db.SaveChangesAsync(cancellationToken);
            dispatched++;
        }

        return dispatched;
    }
}

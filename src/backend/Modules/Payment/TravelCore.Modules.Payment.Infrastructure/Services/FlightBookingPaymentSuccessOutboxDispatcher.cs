using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

/// <summary>
/// Flight Payment-success outbox delivery. No-ops when Flight handler is not registered.
/// </summary>
public sealed class FlightBookingPaymentSuccessOutboxDispatcher
{
    private readonly PaymentDbContext _db;
    private readonly IFlightBookingPaymentSucceededIntegrationHandler? _handler;
    private readonly IClock _clock;

    public FlightBookingPaymentSuccessOutboxDispatcher(
        PaymentDbContext db,
        IClock clock,
        IFlightBookingPaymentSucceededIntegrationHandler? handler = null)
    {
        _db = db;
        _clock = clock;
        _handler = handler;
    }

    public async Task<int> DispatchPendingAsync(int take = 50, CancellationToken cancellationToken = default)
    {
        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take));
        }

        if (_handler is null)
        {
            return 0;
        }

        var pending = await _db.OutboxMessages
            .Where(x => x.ProcessedAt == null
                && x.MessageType == FlightBookingPaymentSuccessOutboxBoundary.MessageType)
            .OrderBy(x => x.OccurredAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var dispatched = 0;
        foreach (var message in pending)
        {
            var evt = FlightBookingPaymentSucceededOutboxSerializer.Deserialize(message.Payload);
            await _handler.HandleAsync(evt, cancellationToken);
            message.MarkProcessed(_clock.GetCurrentInstant());
            await _db.SaveChangesAsync(cancellationToken);
            dispatched++;
        }

        return dispatched;
    }
}

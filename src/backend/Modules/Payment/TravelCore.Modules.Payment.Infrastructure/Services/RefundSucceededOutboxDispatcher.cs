using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

/// <summary>
/// Callable Payment-local RefundSucceeded outbox delivery. At-least-once.
/// </summary>
public sealed class RefundSucceededOutboxDispatcher
{
    private readonly PaymentDbContext _db;
    private readonly IRefundSucceededIntegrationHandler _handler;
    private readonly IClock _clock;

    public RefundSucceededOutboxDispatcher(
        PaymentDbContext db,
        IRefundSucceededIntegrationHandler handler,
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
                && x.MessageType == RefundSuccessOutboxBoundary.MessageType)
            .OrderBy(x => x.OccurredAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var dispatched = 0;
        foreach (var message in pending)
        {
            var evt = RefundSucceededOutboxSerializer.Deserialize(message.Payload);
            await _handler.HandleAsync(evt, cancellationToken);
            message.MarkProcessed(_clock.GetCurrentInstant());
            await _db.SaveChangesAsync(cancellationToken);
            dispatched++;
        }

        return dispatched;
    }
}

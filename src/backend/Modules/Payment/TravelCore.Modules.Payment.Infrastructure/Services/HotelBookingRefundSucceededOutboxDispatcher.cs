using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

/// <summary>
/// Hotel Refund-success outbox delivery. No-ops when HotelBooking handler is not registered.
/// </summary>
public sealed class HotelBookingRefundSucceededOutboxDispatcher
{
    private readonly PaymentDbContext _db;
    private readonly IHotelBookingRefundSucceededIntegrationHandler? _handler;
    private readonly IClock _clock;

    public HotelBookingRefundSucceededOutboxDispatcher(
        PaymentDbContext db,
        IClock clock,
        IHotelBookingRefundSucceededIntegrationHandler? handler = null)
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
                && x.MessageType == HotelBookingRefundSuccessOutboxBoundary.MessageType)
            .OrderBy(x => x.OccurredAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var dispatched = 0;
        foreach (var message in pending)
        {
            var evt = HotelBookingRefundSucceededOutboxSerializer.Deserialize(message.Payload);
            await _handler.HandleAsync(evt, cancellationToken);
            message.MarkProcessed(_clock.GetCurrentInstant());
            await _db.SaveChangesAsync(cancellationToken);
            dispatched++;
        }

        return dispatched;
    }
}

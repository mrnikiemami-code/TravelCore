using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

/// <summary>
/// Hotel Payment-success outbox delivery. No-ops when HotelBooking handler is not registered.
/// </summary>
public sealed class HotelBookingPaymentSuccessOutboxDispatcher
{
    private readonly PaymentDbContext _db;
    private readonly IHotelBookingPaymentSucceededIntegrationHandler? _handler;
    private readonly IClock _clock;

    public HotelBookingPaymentSuccessOutboxDispatcher(
        PaymentDbContext db,
        IClock clock,
        IHotelBookingPaymentSucceededIntegrationHandler? handler = null)
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
                && x.MessageType == HotelBookingPaymentSuccessOutboxBoundary.MessageType)
            .OrderBy(x => x.OccurredAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var dispatched = 0;
        foreach (var message in pending)
        {
            var evt = HotelBookingPaymentSucceededOutboxSerializer.Deserialize(message.Payload);
            await _handler.HandleAsync(evt, cancellationToken);
            message.MarkProcessed(_clock.GetCurrentInstant());
            await _db.SaveChangesAsync(cancellationToken);
            dispatched++;
        }

        return dispatched;
    }
}

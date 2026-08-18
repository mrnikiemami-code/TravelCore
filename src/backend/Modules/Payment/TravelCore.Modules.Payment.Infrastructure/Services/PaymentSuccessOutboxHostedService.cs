using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

/// <summary>
/// Delayed Payment outbox drain. First tick waits a full period so host tests do not race dispatch.
/// </summary>
internal sealed class PaymentSuccessOutboxHostedService : BackgroundService
{
    private static readonly TimeSpan Period = TimeSpan.FromMinutes(1);
    private readonly IServiceScopeFactory _scopes;

    public PaymentSuccessOutboxHostedService(IServiceScopeFactory scopes)
    {
        _scopes = scopes;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Period);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = _scopes.CreateAsyncScope();
                var paymentDispatcher = scope.ServiceProvider.GetRequiredService<PaymentSuccessOutboxDispatcher>();
                await paymentDispatcher.DispatchPendingAsync(take: 50, stoppingToken);
                var hotelPaymentDispatcher = scope.ServiceProvider.GetRequiredService<HotelBookingPaymentSuccessOutboxDispatcher>();
                await hotelPaymentDispatcher.DispatchPendingAsync(take: 50, stoppingToken);
                var refundDispatcher = scope.ServiceProvider.GetRequiredService<RefundSucceededOutboxDispatcher>();
                await refundDispatcher.DispatchPendingAsync(take: 50, stoppingToken);
                var hotelRefundDispatcher = scope.ServiceProvider.GetRequiredService<HotelBookingRefundSucceededOutboxDispatcher>();
                await hotelRefundDispatcher.DispatchPendingAsync(take: 50, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                // Leave rows unprocessed; retry on the next period (at-least-once).
            }
        }
    }
}

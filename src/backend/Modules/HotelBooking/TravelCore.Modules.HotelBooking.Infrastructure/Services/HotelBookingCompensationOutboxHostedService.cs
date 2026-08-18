using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Services;

/// <summary>
/// Delayed HotelBooking compensation and reservation-required outbox drain.
/// </summary>
internal sealed class HotelBookingCompensationOutboxHostedService : BackgroundService
{
    private static readonly TimeSpan Period = TimeSpan.FromMinutes(1);
    private readonly IServiceScopeFactory _scopes;

    public HotelBookingCompensationOutboxHostedService(IServiceScopeFactory scopes)
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
                var compensation = scope.ServiceProvider.GetRequiredService<HotelBookingCompensationOutboxDispatcher>();
                await compensation.DispatchPendingAsync(take: 50, stoppingToken);
                var reservationRequired = scope.ServiceProvider
                    .GetRequiredService<HotelSupplierReservationRequiredOutboxDispatcher>();
                await reservationRequired.DispatchPendingAsync(take: 50, stoppingToken);
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

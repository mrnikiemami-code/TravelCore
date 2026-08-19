using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TravelCore.Modules.Flight.Infrastructure.Services;

/// <summary>
/// Delayed Flight compensation and ticketing-required outbox drain.
/// </summary>
internal sealed class FlightOutboxHostedService : BackgroundService
{
    private static readonly TimeSpan Period = TimeSpan.FromMinutes(1);
    private readonly IServiceScopeFactory _scopes;

    public FlightOutboxHostedService(IServiceScopeFactory scopes)
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
                var compensation = scope.ServiceProvider.GetRequiredService<FlightCompensationOutboxDispatcher>();
                await compensation.DispatchPendingAsync(take: 50, stoppingToken);
                var ticketingRequired = scope.ServiceProvider.GetRequiredService<FlightTicketingRequiredOutboxDispatcher>();
                await ticketingRequired.DispatchPendingAsync(take: 50, stoppingToken);
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

using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace TravelCore.Time;

/// <summary>
/// Host/composition registration for the NodaTime temporal foundation.
/// </summary>
public static class TimeServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IClock"/> as <see cref="SystemClock.Instance"/> for production.
    /// </summary>
    public static IServiceCollection AddTravelCoreTime(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IClock>(SystemClock.Instance);

        return services;
    }
}

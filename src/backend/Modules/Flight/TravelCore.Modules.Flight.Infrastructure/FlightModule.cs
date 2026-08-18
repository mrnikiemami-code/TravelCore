using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;
using TravelCore.Modularity;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Flight.Infrastructure.Reservations;
using TravelCore.Modules.Flight.Infrastructure.Search;
using TravelCore.Modules.Flight.Infrastructure.Services;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Flight.Infrastructure;

/// <summary>
/// Host composition entry for Flight. Zero production sources; zero endpoints.
/// </summary>
public sealed class FlightModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.TryAddSingleton<IClock>(SystemClock.Instance);
        services.AddSingleton<IFlightSearchSourceResolver, FlightSearchSourceResolver>();
        services.AddSingleton<IFlightOfferAvailabilitySourceResolver, FlightOfferAvailabilitySourceResolver>();
        services.AddSingleton<IFlightOfferSourceResolver, FlightOfferSourceResolver>();
        services.AddSingleton<IFlightReservationSourceResolver, FlightReservationSourceResolver>();
        services.AddScoped<FlightLiveSearchService>();
        services.AddScoped<FlightOfferAcceptanceService>();
        services.AddScoped<FlightSupplierReservationService>();

        services.AddDbContext<FlightDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use FlightDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: FlightDbContext.SchemaName);
        });
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
    }
}

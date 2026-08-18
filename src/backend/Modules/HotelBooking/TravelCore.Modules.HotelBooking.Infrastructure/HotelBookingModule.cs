using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modularity;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.HotelBooking.Infrastructure;

/// <summary>
/// Host composition entry for HotelBooking (TC-P21-T001 / P21-R1). Schema ownership only — no endpoints.
/// </summary>
public sealed class HotelBookingModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<HotelBookingDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use HotelBookingDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: HotelBookingDbContext.SchemaName);
        });
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
    }
}

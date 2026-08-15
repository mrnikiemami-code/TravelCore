using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;
using TravelCore.Modularity;
using TravelCore.Modules.Party.Contracts;
using TravelCore.Modules.Party.Infrastructure.Endpoints;
using TravelCore.Modules.Party.Infrastructure.Services;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Party.Infrastructure;

/// <summary>
/// Host composition entry for the Party module.
/// </summary>
public sealed class PartyModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddValidation();
        services.TryAddSingleton<IClock>(SystemClock.Instance);

        services.AddDbContext<PartyDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use PartyDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: PartyDbContext.SchemaName);
        });

        services.AddScoped<PartyApplicationService>();
        services.AddScoped<IPartyExistenceQuery, PartyExistenceQuery>();
        services.AddScoped<IPartyReadQuery, PartyReadQuery>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapPartyEndpoints();
    }
}

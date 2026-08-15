using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;
using TravelCore.Modularity;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.Identity.Infrastructure.Endpoints;
using TravelCore.Modules.Identity.Infrastructure.Security;
using TravelCore.Modules.Identity.Infrastructure.Services;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Identity.Infrastructure;

/// <summary>
/// Host composition entry for the Identity module.
/// </summary>
public sealed class IdentityModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddValidation();
        services.TryAddSingleton<IClock>(SystemClock.Instance);
        services.TryAddSingleton<IIdentityPasswordHasher, AspNetCoreIdentityPasswordHasher>();

        services.AddDbContext<IdentityDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use IdentityDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: IdentityDbContext.SchemaName);
        });

        services.AddScoped<IdentityApplicationService>();
        services.AddScoped<IAccountExistenceQuery, AccountExistenceQuery>();
        services.AddTravelCoreIdentityCookieAuthentication();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapIdentityEndpoints();
    }
}

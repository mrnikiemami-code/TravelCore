using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modularity;
using TravelCore.Modules.Pricing.Contracts;
using TravelCore.Modules.Pricing.Infrastructure.Endpoints;
using TravelCore.Modules.Pricing.Infrastructure.Services;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Pricing.Infrastructure;

/// <summary>
/// Host composition entry for the Pricing module (Admin — T006; FX stub — T007; public read — T008).
/// </summary>
public sealed class PricingModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<PricingDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use PricingDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: PricingDbContext.SchemaName);
        });

        services.AddScoped<IPriceAdminService, PriceAdminService>();
        services.AddScoped<IPublicPricingQuery, PublicPricingQuery>();
        services.AddScoped<IAuthoritativeQuoteQuery, AuthoritativeQuoteQuery>();
        services.AddScoped<IAuthoritativeQuoteIssuer, AuthoritativeQuoteIssuer>();
        services.AddSingleton<IFxConversionPort, FxBoundaryUnavailablePort>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapPricingAdminEndpoints();
        endpoints.MapPricingPublicEndpoints();
    }
}

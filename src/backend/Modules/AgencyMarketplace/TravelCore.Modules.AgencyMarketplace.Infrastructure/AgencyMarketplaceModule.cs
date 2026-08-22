using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modularity;
using TravelCore.Modules.AgencyMarketplace.Contracts;
using TravelCore.Modules.AgencyMarketplace.Infrastructure.Endpoints;
using TravelCore.Modules.AgencyMarketplace.Infrastructure.Policies;
using TravelCore.Modules.AgencyMarketplace.Infrastructure.Services;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure;

/// <summary>
/// Host composition entry for the Agency Marketplace module
/// (panel — TC-P13-T006; public related offers — TC-P14-T007).
/// </summary>
public sealed class AgencyMarketplaceModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<AgencyMarketplaceDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use AgencyMarketplaceDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: AgencyMarketplaceDbContext.SchemaName);
        });

        services.AddScoped<IAgencyMarketplacePanelService, AgencyMarketplacePanelService>();
        services.AddScoped<IRelatedAgencyOfferPublicQuery, RelatedAgencyOfferPublicQuery>();
        services.AddScoped<IAgencyOriginContextQuery, AgencyOriginContextQuery>();
        services.AddScoped<IAgencyOfferGovernanceService, AgencyOfferGovernanceService>();
        services.AddScoped<IAgencyOfferCommercialPolicy, AllowAgencyOfferCommercialPolicy>();
        services.AddScoped<IAgencyOfferContentPolicy, AllowAgencyOfferContentPolicy>();
        services.AddScoped<IAgencyOfferChannelPolicy, AllowAgencyOfferChannelPolicy>();
        services.AddScoped<IAgencyOfferPublicationPolicy, AllowAgencyOfferPublicationPolicy>();
        services.AddScoped<IAgencyOfferPolicyEvaluator, AgencyOfferPolicyEvaluator>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapAgencyMarketplacePanelEndpoints();
        endpoints.MapAgencyMarketplaceAdminEndpoints();
        endpoints.MapAgencyMarketplacePublicEndpoints();
    }
}

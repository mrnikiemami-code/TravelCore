using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;
using TravelCore.Modularity;
using TravelCore.Modules.Access.Infrastructure.Authorization;
using TravelCore.Modules.Access.Infrastructure.Endpoints;
using TravelCore.Modules.Access.Infrastructure.Services;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Access.Infrastructure;

/// <summary>
/// Host composition entry for the Access module.
/// </summary>
public sealed class AccessModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddValidation();
        services.TryAddSingleton<IClock>(SystemClock.Instance);

        services.AddDbContext<AccessDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use AccessDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: AccessDbContext.SchemaName);
        });

        services.AddScoped<AccessTaxonomyService>();
        services.AddScoped<AccessSubjectAssignmentService>();
        services.AddScoped<IAccessAuthorizationEvaluator, AccessAuthorizationEvaluator>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddAuthorizationBuilder()
            .AddPolicy(AccessAuthorizationPolicies.AdminRolesRead, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement("access.roles.read"));
            })
            .AddPolicy(AccessAuthorizationPolicies.AgencyPanelOpen, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement("agency.panel.open"));
            })
            .AddPolicy(AccessAuthorizationPolicies.DestinationDestinationsWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement("destination.destinations.write"));
            })
            .AddPolicy(AccessAuthorizationPolicies.SeoDestinationPostureWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement("seo.destination-posture.write"));
            })
            .AddPolicy(AccessAuthorizationPolicies.MediaAssetsWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement("media.assets.write"));
            })
            .AddPolicy(AccessAuthorizationPolicies.PlacePlacesWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement("place.places.write"));
            })
            .AddPolicy(AccessAuthorizationPolicies.SeoPlacePostureWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement("seo.place-posture.write"));
            })
            .AddPolicy(AccessAuthorizationPolicies.ContentItemsWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement("content.items.write"));
            })
            .AddPolicy(AccessAuthorizationPolicies.SeoContentPostureWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement("seo.content-posture.write"));
            })
            .AddPolicy(AccessAuthorizationPolicies.TourProductsWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement("tour.products.write"));
            })
            .AddPolicy(AccessAuthorizationPolicies.TourDeparturesRead, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement("tour.departures.read"));
            })
            .AddPolicy(AccessAuthorizationPolicies.TourDeparturesWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement("tour.departures.write"));
            })
            .AddPolicy(AccessAuthorizationPolicies.SeoTourPostureWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement("seo.tour-posture.write"));
            })
            .AddPolicy(AccessAuthorizationPolicies.PricingPricesRead, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement("pricing.prices.read"));
            })
            .AddPolicy(AccessAuthorizationPolicies.PricingPricesWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement("pricing.prices.write"));
            })
            .AddPolicy(AccessAuthorizationPolicies.AgencyMarketplaceProfileRead, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement("agency.marketplace.profile.read"));
            })
            .AddPolicy(AccessAuthorizationPolicies.AgencyMarketplaceProfileWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement("agency.marketplace.profile.write"));
            })
            .AddPolicy(AccessAuthorizationPolicies.AgencyMarketplaceOffersRead, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement("agency.marketplace.offers.read"));
            })
            .AddPolicy(AccessAuthorizationPolicies.AgencyMarketplaceOffersWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement("agency.marketplace.offers.write"));
            })
            .AddPolicy(AccessAuthorizationPolicies.AgencyMarketplaceOffersModerate, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement("agency.marketplace.offers.moderate"));
            });
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapAccessEndpoints();
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TravelCore.Modularity;

/// <summary>
/// Compile-time / host-explicit module composition helpers.
/// Modules are selected by the host; there is no assembly scanning or reflection discovery.
/// </summary>
public static class ModuleComposition
{
    public static IServiceCollection AddTravelCoreModules(
        this IServiceCollection services,
        IConfiguration configuration,
        IReadOnlyList<ITravelCoreModule> modules)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(modules);

        foreach (var module in modules)
        {
            ArgumentNullException.ThrowIfNull(module);
            module.RegisterServices(services, configuration);
        }

        return services;
    }

    public static WebApplication MapTravelCoreModules(
        this WebApplication app,
        IReadOnlyList<ITravelCoreModule> modules)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(modules);

        IEndpointRouteBuilder endpoints = app;
        foreach (var module in modules)
        {
            ArgumentNullException.ThrowIfNull(module);
            module.MapEndpoints(endpoints);
        }

        return app;
    }
}

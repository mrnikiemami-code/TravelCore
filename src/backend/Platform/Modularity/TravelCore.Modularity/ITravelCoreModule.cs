using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TravelCore.Modularity;

/// <summary>
/// Explicit composition entry for a TravelCore module.
/// Domain projects must not reference this contract; composition/infrastructure entry points may.
/// </summary>
public interface ITravelCoreModule
{
    void RegisterServices(IServiceCollection services, IConfiguration configuration);

    void MapEndpoints(IEndpointRouteBuilder endpoints);
}

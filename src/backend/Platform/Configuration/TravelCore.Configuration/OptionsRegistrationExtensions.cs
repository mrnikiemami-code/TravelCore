using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TravelCore.Configuration;

/// <summary>
/// Explicit, capability-owned Options registration.
/// Section names are never derived by reflection from type names.
/// </summary>
public static class OptionsRegistrationExtensions
{
    /// <summary>
    /// Binds <typeparamref name="TOptions"/> to an explicit configuration section and enables
    /// DataAnnotations + ValidateOnStart. Call only when the owning capability requires that section.
    /// </summary>
    public static OptionsBuilder<TOptions> AddTravelCoreOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
        where TOptions : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);

        return services
            .AddOptions<TOptions>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}

using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;
using TravelCore.Modularity;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Infrastructure.Endpoints;
using TravelCore.Modules.Payment.Infrastructure.Options;
using TravelCore.Modules.Payment.Infrastructure.Providers;
using TravelCore.Modules.Payment.Infrastructure.Services;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Payment.Infrastructure;

/// <summary>
/// Host composition entry for Payment (TC-P20-T003 / P20-R3).
/// Registers provider-neutral ports. Does not register a production fake provider.
/// </summary>
public sealed class PaymentModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.TryAddSingleton<IClock>(SystemClock.Instance);
        services.AddOptions<PaymentProviderOptions>()
            .Bind(configuration.GetSection(PaymentProviderOptions.SectionName));
        services.AddSingleton<IPaymentProviderResolver, PaymentProviderResolver>();
        services.AddScoped<PaymentGetOrCreateService>();
        services.AddScoped<PaymentInitiationService>();
        services.AddScoped<PaymentCallbackProcessor>();
        services.AddScoped<PaymentAttemptRecheckService>();

        services.AddDbContext<PaymentDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use PaymentDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: PaymentDbContext.SchemaName);
        });
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapPaymentProviderCallbackEndpoints();
    }
}

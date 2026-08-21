using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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
/// Sandbox adapter may register only under non-production + Payment:Sandbox:Enabled (TC-P34-T003).
/// Stripe TEST-MODE adapter may register only under non-production + Payment:Stripe:Enabled + sk_test_ (TC-P35-T008).
/// NamedProductionAdapterImplemented remains false.
/// </summary>
public sealed class PaymentModule : ITravelCoreModule
{
    private bool _sandboxRegistered;

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.TryAddSingleton<IClock>(SystemClock.Instance);
        services.AddOptions<PaymentProviderOptions>()
            .Bind(configuration.GetSection(PaymentProviderOptions.SectionName));
        services.AddSingleton<IPaymentProviderResolver, PaymentProviderResolver>();
        services.AddScoped<PaymentGetOrCreateService>();
        services.AddScoped(sp => new PaymentPreparationService(
            sp.GetRequiredService<PaymentDbContext>(),
            sp.GetRequiredService<TravelCore.Modules.Booking.Contracts.IBookingPaymentObligationQuery>(),
            sp.GetRequiredService<IClock>(),
            sp.GetService<TravelCore.Modules.HotelBooking.Contracts.IHotelBookingPaymentObligationQuery>(),
            sp.GetService<TravelCore.Modules.Flight.Contracts.IFlightBookingPaymentObligationQuery>()));
        services.AddScoped<PaymentInitiationService>();
        services.AddScoped<IHotelBookingPaymentInitiationService>(sp => sp.GetRequiredService<PaymentInitiationService>());
        services.AddScoped<IFlightBookingPaymentInitiationService>(sp => sp.GetRequiredService<PaymentInitiationService>());
        services.AddScoped<PaymentCallbackProcessor>();
        services.AddScoped<PaymentAttemptRecheckService>();
        services.AddScoped<PaymentSuccessOutboxDispatcher>();
        services.AddScoped(sp => new HotelBookingPaymentSuccessOutboxDispatcher(
            sp.GetRequiredService<PaymentDbContext>(),
            sp.GetRequiredService<IClock>(),
            sp.GetService<IHotelBookingPaymentSucceededIntegrationHandler>()));
        services.AddScoped(sp => new FlightBookingPaymentSuccessOutboxDispatcher(
            sp.GetRequiredService<PaymentDbContext>(),
            sp.GetRequiredService<IClock>(),
            sp.GetService<IFlightBookingPaymentSucceededIntegrationHandler>()));
        services.AddScoped<RefundGetOrCreateService>();
        services.AddScoped<RefundInitiationService>();
        services.AddScoped<RefundAttemptRecheckService>();
        services.AddScoped<RefundSucceededOutboxDispatcher>();
        services.AddScoped(sp => new HotelBookingRefundSucceededOutboxDispatcher(
            sp.GetRequiredService<PaymentDbContext>(),
            sp.GetRequiredService<IClock>(),
            sp.GetService<IHotelBookingRefundSucceededIntegrationHandler>()));
        services.AddScoped(sp => new FlightBookingRefundSucceededOutboxDispatcher(
            sp.GetRequiredService<PaymentDbContext>(),
            sp.GetRequiredService<IClock>(),
            sp.GetService<IFlightBookingRefundSucceededIntegrationHandler>()));
        services.AddScoped<IBookingPaymentCompensationRequiredHandler, BookingPaymentCompensationRequiredHandler>();
        services.AddScoped<IHotelBookingPaymentCompensationRequiredHandler, HotelBookingPaymentCompensationRequiredHandler>();
        services.AddScoped<IFlightBookingPaymentCompensationRequiredHandler, FlightBookingPaymentCompensationRequiredHandler>();
        services.AddScoped<IHotelBookingCancellationRefundRequiredHandler, HotelBookingCancellationRefundRequiredHandler>();
        services.AddScoped<IFlightBookingCancellationRefundRequiredHandler, FlightBookingCancellationRefundRequiredHandler>();
        services.AddHostedService<PaymentSuccessOutboxHostedService>();
        services.AddScoped<IPaymentSuccessEvidenceQuery, PaymentSuccessEvidenceQueryService>();
        services.AddScoped<IPublicBookingPaymentService, PublicBookingPaymentService>();
        services.AddScoped<IPublicHotelBookingPaymentService, PublicHotelBookingPaymentService>();
        services.AddScoped<IPublicFlightBookingPaymentService, PublicFlightBookingPaymentService>();
        services.AddScoped<IPaymentOperationalQuery, PaymentOperationalQueryService>();

        TryRegisterSandbox(services, configuration);
        TryRegisterStripe(services, configuration);

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
        if (_sandboxRegistered)
        {
            endpoints.MapSandboxPaymentOutcomeEndpoints();
        }
    }

    private void TryRegisterSandbox(IServiceCollection services, IConfiguration configuration)
    {
        var environmentName = ResolveHostEnvironmentName(services)
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

        // Fail closed when environment cannot be determined — never register sandbox by accident.
        var enabled = configuration.GetValue($"{PaymentSandboxOptions.SectionName}:Enabled", false);
        if (!PaymentSandboxGate.IsAllowed(environmentName, enabled))
        {
            _sandboxRegistered = false;
            return;
        }

        services.AddOptions<PaymentSandboxOptions>()
            .Bind(configuration.GetSection(PaymentSandboxOptions.SectionName));
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<SandboxPaymentSessionStore>();
        // TryAddEnumerable keeps architecture guard against unconditional gateway singleton registration.
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IPaymentProviderGateway, SandboxPaymentProviderGateway>());
        services.AddHttpClient("TravelCore.PaymentSandbox");
        _sandboxRegistered = true;
    }

    private void TryRegisterStripe(IServiceCollection services, IConfiguration configuration)
    {
        var environmentName = ResolveHostEnvironmentName(services)
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

        var enabled = configuration.GetValue($"{PaymentStripeOptions.SectionName}:Enabled", false);
        var secretKey = configuration[$"{PaymentStripeOptions.SectionName}:SecretKey"];
        if (!PaymentStripeGate.IsAllowed(environmentName, enabled, secretKey))
        {
            return;
        }

        services.AddOptions<PaymentStripeOptions>()
            .Bind(configuration.GetSection(PaymentStripeOptions.SectionName));
        services.AddSingleton<IStripeCheckoutClient, StripeNetCheckoutClient>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IPaymentProviderGateway, StripePaymentProviderGateway>());
    }

    private static string? ResolveHostEnvironmentName(IServiceCollection services)
    {
        foreach (var descriptor in services)
        {
            if (descriptor.ServiceType == typeof(IHostEnvironment)
                && descriptor.ImplementationInstance is IHostEnvironment hostEnvironment)
            {
                return hostEnvironment.EnvironmentName;
            }

            if (descriptor.ServiceType == typeof(IWebHostEnvironment)
                && descriptor.ImplementationInstance is IWebHostEnvironment webHostEnvironment)
            {
                return webHostEnvironment.EnvironmentName;
            }
        }

        return null;
    }
}

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
        services.AddScoped(sp => new PaymentPreparationService(
            sp.GetRequiredService<PaymentDbContext>(),
            sp.GetRequiredService<TravelCore.Modules.Booking.Contracts.IBookingPaymentObligationQuery>(),
            sp.GetRequiredService<IClock>(),
            sp.GetService<TravelCore.Modules.HotelBooking.Contracts.IHotelBookingPaymentObligationQuery>()));
        services.AddScoped<PaymentInitiationService>();
        services.AddScoped<IHotelBookingPaymentInitiationService>(sp => sp.GetRequiredService<PaymentInitiationService>());
        services.AddScoped<PaymentCallbackProcessor>();
        services.AddScoped<PaymentAttemptRecheckService>();
        services.AddScoped<PaymentSuccessOutboxDispatcher>();
        services.AddScoped(sp => new HotelBookingPaymentSuccessOutboxDispatcher(
            sp.GetRequiredService<PaymentDbContext>(),
            sp.GetRequiredService<IClock>(),
            sp.GetService<IHotelBookingPaymentSucceededIntegrationHandler>()));
        services.AddScoped<RefundGetOrCreateService>();
        services.AddScoped<RefundInitiationService>();
        services.AddScoped<RefundAttemptRecheckService>();
        services.AddScoped<RefundSucceededOutboxDispatcher>();
        services.AddScoped(sp => new HotelBookingRefundSucceededOutboxDispatcher(
            sp.GetRequiredService<PaymentDbContext>(),
            sp.GetRequiredService<IClock>(),
            sp.GetService<IHotelBookingRefundSucceededIntegrationHandler>()));
        services.AddScoped<IBookingPaymentCompensationRequiredHandler, BookingPaymentCompensationRequiredHandler>();
        services.AddScoped<IHotelBookingPaymentCompensationRequiredHandler, HotelBookingPaymentCompensationRequiredHandler>();
        services.AddScoped<IHotelBookingCancellationRefundRequiredHandler, HotelBookingCancellationRefundRequiredHandler>();
        services.AddHostedService<PaymentSuccessOutboxHostedService>();
        services.AddScoped<IPaymentSuccessEvidenceQuery, PaymentSuccessEvidenceQueryService>();
        services.AddScoped<IPublicBookingPaymentService, PublicBookingPaymentService>();
        services.AddScoped<IPaymentOperationalQuery, PaymentOperationalQueryService>();

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

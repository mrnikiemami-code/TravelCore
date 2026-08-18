using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;
using TravelCore.Modularity;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Infrastructure.Endpoints;
using TravelCore.Modules.HotelBooking.Infrastructure.Availability;
using TravelCore.Modules.HotelBooking.Infrastructure.Rates;
using TravelCore.Modules.HotelBooking.Infrastructure.Reservations;
using TravelCore.Modules.HotelBooking.Infrastructure.Services;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.HotelBooking.Infrastructure;

/// <summary>
/// Host composition for HotelBooking. Schema, stay aggregate, availability hold, rate-offer snapshot,
/// supplier reservation lifecycle, and public transactional journey (P21-R8).
/// Production availability/rate/reservation sources remain NONE.
/// </summary>
public sealed class HotelBookingModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.TryAddSingleton<IClock>(SystemClock.Instance);
        services.AddSingleton<IHotelAvailabilitySourceResolver, HotelAvailabilitySourceResolver>();
        services.AddScoped<HotelAvailabilityHoldService>();
        services.AddSingleton<IHotelRateOfferSourceResolver, HotelRateOfferSourceResolver>();
        services.AddScoped<HotelRateOfferAcceptanceService>();
        services.AddSingleton<IHotelReservationSourceResolver, HotelReservationSourceResolver>();
        services.AddScoped<HotelSupplierReservationService>();
        services.AddScoped<HotelBookingCancellationService>();
        services.AddScoped<HotelBookingPaymentObligationQueryService>();
        services.AddScoped<IHotelBookingPaymentObligationQuery>(
            sp => sp.GetRequiredService<HotelBookingPaymentObligationQueryService>());
        services.AddScoped<IHotelBookingPaymentSucceededIntegrationHandler, HotelBookingPaymentSucceededIntegrationHandler>();
        services.AddScoped<IHotelBookingRefundSucceededIntegrationHandler, HotelBookingRefundSucceededIntegrationHandler>();
        services.AddScoped<HotelBookingCompensationOutboxDispatcher>();
        services.AddScoped<HotelBookingCancellationRefundOutboxDispatcher>();
        services.AddScoped<HotelSupplierReservationRequiredOutboxDispatcher>();
        services.AddScoped<PlaceContractHotelCatalogLookup>();
        services.AddScoped<IHotelPlaceCatalogLookup>(sp => sp.GetRequiredService<PlaceContractHotelCatalogLookup>());
        services.AddSingleton<IHotelSourceCatalog, HotelSourceCatalog>();
        services.AddScoped<PublicHotelBookingSurfaceService>();
        services.AddScoped<IPublicHotelBookingInitiationService>(
            sp => sp.GetRequiredService<PublicHotelBookingSurfaceService>());
        services.AddScoped<IPublicHotelBookingReadService>(
            sp => sp.GetRequiredService<PublicHotelBookingSurfaceService>());
        services.AddScoped<IPublicHotelBookingJourneyService>(
            sp => sp.GetRequiredService<PublicHotelBookingSurfaceService>());
        services.AddScoped<HotelBookingOperationalQueryService>();
        services.AddScoped<IHotelBookingOperationalQuery>(
            sp => sp.GetRequiredService<HotelBookingOperationalQueryService>());
        services.AddHostedService<HotelBookingCompensationOutboxHostedService>();

        services.AddDbContext<HotelBookingDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use HotelBookingDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: HotelBookingDbContext.SchemaName);
        });
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapPublicHotelBookingEndpoints();
    }
}

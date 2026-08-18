using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;
using TravelCore.Modularity;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Infrastructure.Endpoints;
using TravelCore.Modules.Booking.Infrastructure.Services;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Booking.Infrastructure;

/// <summary>
/// Host composition entry for Booking (TC-P19-T008 / P19-R8 public initiation).
/// </summary>
public sealed class BookingModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<BookingDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use BookingDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: BookingDbContext.SchemaName);
        });
        services.TryAddSingleton<IClock>(SystemClock.Instance);
        services.AddScoped<BookingCapacityService>();
        services.AddScoped<BookingPeopleService>();
        services.AddScoped<BookingQuoteService>();
        services.AddScoped<BookingCancellationService>();
        services.AddScoped<BookingPaymentConfirmationService>();
        services.AddScoped<IPaymentSucceededIntegrationHandler, BookingPaymentSucceededIntegrationHandler>();
        services.AddScoped<BookingPaymentObligationQueryService>();
        services.AddScoped<BookingCreationService>();
        services.AddScoped<PublicBookingSurfaceService>();
        services.AddScoped<IPublicBookingInitiationService>(sp => sp.GetRequiredService<PublicBookingSurfaceService>());
        services.AddScoped<IPublicBookingReadService>(sp => sp.GetRequiredService<PublicBookingSurfaceService>());
        services.AddScoped<IBookingPaymentObligationQuery>(sp => sp.GetRequiredService<BookingPaymentObligationQueryService>());
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapPublicBookingEndpoints();
    }
}

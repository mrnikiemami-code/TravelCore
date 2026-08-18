using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modularity;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Booking.Infrastructure;

/// <summary>
/// Host composition entry for Booking (TC-P19-T001 / P19-R1 scaffolding only).
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
        services.AddScoped<TravelCore.Modules.Booking.Infrastructure.Services.BookingCapacityService>();
        services.AddScoped<TravelCore.Modules.Booking.Infrastructure.Services.BookingPeopleService>();
        services.AddScoped<TravelCore.Modules.Booking.Infrastructure.Services.BookingQuoteService>();
        services.AddScoped<TravelCore.Modules.Booking.Infrastructure.Services.BookingCancellationService>();
        services.AddScoped<TravelCore.Modules.Booking.Infrastructure.Services.BookingCreationService>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
    }
}

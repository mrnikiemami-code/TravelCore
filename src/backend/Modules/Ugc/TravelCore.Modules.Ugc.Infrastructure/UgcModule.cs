using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modularity;
using TravelCore.Modules.Ugc.Contracts;
using TravelCore.Modules.Ugc.Infrastructure.Services;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Ugc.Infrastructure;

/// <summary>
/// Host composition entry for UGC (TC-P16-T001). Schema scaffolding only — no product endpoints.
/// </summary>
public sealed class UgcModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<UgcDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use UgcDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: UgcDbContext.SchemaName);
        });

        services.AddSingleton<IReviewTargetValidator, StructuralReviewTargetValidator>();
        services.AddSingleton<IUserPhotoMediaAssetValidator, StructuralUserPhotoMediaAssetValidator>();
        services.AddSingleton<ICommentTargetValidator, StructuralCommentTargetValidator>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
    }
}

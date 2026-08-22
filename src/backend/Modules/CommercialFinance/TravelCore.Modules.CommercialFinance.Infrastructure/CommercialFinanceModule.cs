using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modularity;
using TravelCore.Modules.CommercialFinance.Contracts;
using TravelCore.Modules.CommercialFinance.Infrastructure.Endpoints;
using TravelCore.Modules.CommercialFinance.Infrastructure.Services;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.CommercialFinance.Infrastructure;

/// <summary>
/// Host composition entry for Commercial Finance (TC-P39-T006).
/// Persistence skeleton + read-only admin endpoints; no Payment event handlers or payout execution.
/// </summary>
public sealed class CommercialFinanceModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<CommercialFinanceDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use CommercialFinanceDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: CommercialFinanceDbContext.SchemaName);
        });

        services.AddScoped<ICommercialFinanceAgreementQuery, CommercialFinanceAgreementQuery>();
        services.AddScoped<ICommercialFinanceObligationQuery, CommercialFinanceObligationQuery>();
        services.AddScoped<ICommercialFinanceSettlementQuery, CommercialFinanceSettlementQuery>();
        services.AddScoped<ICommercialFinancePayoutQuery, CommercialFinancePayoutQuery>();
        services.AddScoped<ICommercialFinanceEvidencePort, NullCommercialFinanceEvidencePort>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapCommercialFinanceAdminEndpoints();
    }
}

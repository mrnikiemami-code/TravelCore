using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.CommercialFinance.Infrastructure;

/// <summary>
/// Design-time factory for EF tooling only (dotnet-ef). Fake non-secret connection string.
/// </summary>
public sealed class CommercialFinanceDbContextFactory : IDesignTimeDbContextFactory<CommercialFinanceDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_commercial_finance_design;Username=travelcore_design;Password=not-a-real-secret";

    public CommercialFinanceDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CommercialFinanceDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: CommercialFinanceDbContext.SchemaName)
            .Options;

        return new CommercialFinanceDbContext(options);
    }
}

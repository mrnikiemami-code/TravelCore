using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Pricing.Infrastructure;

/// <summary>
/// Design-time factory for EF tooling only (dotnet-ef). Fake non-secret connection string.
/// </summary>
public sealed class PricingDbContextFactory : IDesignTimeDbContextFactory<PricingDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_pricing_design;Username=travelcore_design;Password=not-a-real-secret";

    public PricingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PricingDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: PricingDbContext.SchemaName)
            .Options;

        return new PricingDbContext(options);
    }
}

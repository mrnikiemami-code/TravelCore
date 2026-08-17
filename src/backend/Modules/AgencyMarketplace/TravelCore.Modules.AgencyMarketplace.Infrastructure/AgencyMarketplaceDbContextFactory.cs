using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure;

/// <summary>
/// Design-time factory for EF tooling only (dotnet-ef). Fake non-secret connection string.
/// </summary>
public sealed class AgencyMarketplaceDbContextFactory : IDesignTimeDbContextFactory<AgencyMarketplaceDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_agency_marketplace_design;Username=travelcore_design;Password=not-a-real-secret";

    public AgencyMarketplaceDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AgencyMarketplaceDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: AgencyMarketplaceDbContext.SchemaName)
            .Options;

        return new AgencyMarketplaceDbContext(options);
    }
}

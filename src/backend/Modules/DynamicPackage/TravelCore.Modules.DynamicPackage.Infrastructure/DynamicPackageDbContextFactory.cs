using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.DynamicPackage.Infrastructure;

/// <summary>
/// Design-time factory for EF tooling only (dotnet-ef). Fake non-secret connection string.
/// </summary>
public sealed class DynamicPackageDbContextFactory : IDesignTimeDbContextFactory<DynamicPackageDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_dynamic_package_design;Username=travelcore_design;Password=not-a-real-secret";

    public DynamicPackageDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DynamicPackageDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: DynamicPackageDbContext.SchemaName)
            .Options;

        return new DynamicPackageDbContext(options);
    }
}

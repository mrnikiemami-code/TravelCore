using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.B2B.Infrastructure;

/// <summary>
/// Design-time factory for EF tooling only (dotnet-ef). Fake non-secret connection string.
/// </summary>
public sealed class B2BDbContextFactory : IDesignTimeDbContextFactory<B2BDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_b2b_design;Username=travelcore_design;Password=not-a-real-secret";

    public B2BDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<B2BDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: B2BDbContext.SchemaName)
            .Options;

        return new B2BDbContext(options);
    }
}

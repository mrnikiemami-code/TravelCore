using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Access.Infrastructure;

public sealed class AccessDbContextFactory : IDesignTimeDbContextFactory<AccessDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_access_design;Username=travelcore_design;Password=not-a-real-secret";

    public AccessDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AccessDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: AccessDbContext.SchemaName)
            .Options;
        return new AccessDbContext(options);
    }
}

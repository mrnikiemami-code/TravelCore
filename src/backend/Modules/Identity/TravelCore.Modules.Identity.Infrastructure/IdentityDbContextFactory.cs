using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Identity.Infrastructure;

/// <summary>
/// Design-time factory for EF tooling only. Fake non-secret connection string.
/// </summary>
public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_identity_design;Username=travelcore_design;Password=not-a-real-secret";

    public IdentityDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: IdentityDbContext.SchemaName)
            .Options;

        return new IdentityDbContext(options);
    }
}

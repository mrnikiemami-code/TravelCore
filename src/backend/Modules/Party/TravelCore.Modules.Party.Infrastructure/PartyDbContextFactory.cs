using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Party.Infrastructure;

/// <summary>
/// Design-time factory for EF tooling only (dotnet-ef). Fake non-secret connection string.
/// </summary>
public sealed class PartyDbContextFactory : IDesignTimeDbContextFactory<PartyDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_party_design;Username=travelcore_design;Password=not-a-real-secret";

    public PartyDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PartyDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: PartyDbContext.SchemaName)
            .Options;

        return new PartyDbContext(options);
    }
}

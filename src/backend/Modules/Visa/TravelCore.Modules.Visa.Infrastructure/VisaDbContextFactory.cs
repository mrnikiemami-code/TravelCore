using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Visa.Infrastructure;

/// <summary>
/// Design-time factory for EF tooling only (dotnet-ef). Fake non-secret connection string.
/// </summary>
public sealed class VisaDbContextFactory : IDesignTimeDbContextFactory<VisaDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_visa_design;Username=travelcore_design;Password=not-a-real-secret";

    public VisaDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<VisaDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: VisaDbContext.SchemaName)
            .Options;

        return new VisaDbContext(options);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Payment.Infrastructure;

/// <summary>
/// Design-time factory for EF tooling only (dotnet-ef). Fake non-secret connection string.
/// </summary>
public sealed class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_payment_design;Username=travelcore_design;Password=not-a-real-secret";

    public PaymentDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: PaymentDbContext.SchemaName)
            .Options;

        return new PaymentDbContext(options);
    }
}

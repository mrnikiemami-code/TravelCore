using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Payment.Infrastructure;

/// <summary>
/// Payment-owned DbContext. Owns PostgreSQL schema <c>payment</c>.
/// No product tables in T001 (P20-R1 scaffolding only).
/// </summary>
public sealed class PaymentDbContext : DbContext
{
    public const string SchemaName = "payment";

    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
    }
}

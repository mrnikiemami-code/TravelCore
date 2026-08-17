using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Pricing.Infrastructure;

/// <summary>
/// Pricing-owned DbContext. Owns PostgreSQL schema <c>pricing</c>.
/// Scaffolding only — no product entities yet (TC-P12-T001 / P12-R1).
/// </summary>
public sealed class PricingDbContext : DbContext
{
    public const string SchemaName = "pricing";

    public PricingDbContext(DbContextOptions<PricingDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PricingDbContext).Assembly);
    }
}

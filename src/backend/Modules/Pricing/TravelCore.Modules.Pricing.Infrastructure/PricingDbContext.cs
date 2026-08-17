using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Pricing.Domain;

namespace TravelCore.Modules.Pricing.Infrastructure;

/// <summary>
/// Pricing-owned DbContext. Owns PostgreSQL schema <c>pricing</c>.
/// Maps <see cref="Price"/> / <see cref="PriceComponent"/> with logical TargetType+TargetId (no Tour FK).
/// </summary>
public sealed class PricingDbContext : DbContext
{
    public const string SchemaName = "pricing";

    public PricingDbContext(DbContextOptions<PricingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Price> Prices => Set<Price>();

    public DbSet<PriceComponent> PriceComponents => Set<PriceComponent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PricingDbContext).Assembly);
    }
}

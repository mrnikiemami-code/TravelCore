using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Pricing.Domain;

namespace TravelCore.Modules.Pricing.Infrastructure;

/// <summary>
/// Pricing-owned DbContext. Owns PostgreSQL schema <c>pricing</c>.
/// Maps Price / PriceComponent / PriceOccupancyRule and Quote / QuoteSnapshotComponent (no Tour/Booking/Payment FK).
/// P12-R7: no ExchangeRate table; Quote may store optional requested display-currency metadata only.
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
    public DbSet<PriceOccupancyRule> PriceOccupancyRules => Set<PriceOccupancyRule>();

    public DbSet<Quote> Quotes => Set<Quote>();

    public DbSet<QuoteSnapshotComponent> QuoteSnapshotComponents => Set<QuoteSnapshotComponent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PricingDbContext).Assembly);
    }
}

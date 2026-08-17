using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure;

/// <summary>
/// Agency Marketplace-owned DbContext. Owns PostgreSQL schema <c>agency_marketplace</c>.
/// Scaffolding only — no product entities yet (TC-P13-T001 / P13-R1).
/// </summary>
public sealed class AgencyMarketplaceDbContext : DbContext
{
    public const string SchemaName = "agency_marketplace";

    public AgencyMarketplaceDbContext(DbContextOptions<AgencyMarketplaceDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgencyMarketplaceDbContext).Assembly);
    }
}

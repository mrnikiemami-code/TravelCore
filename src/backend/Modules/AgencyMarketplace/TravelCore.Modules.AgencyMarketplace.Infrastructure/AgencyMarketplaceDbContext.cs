using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.AgencyMarketplace.Domain;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure;

/// <summary>
/// Agency Marketplace-owned DbContext. Owns PostgreSQL schema <c>agency_marketplace</c>.
/// Maps AgencyProfile (P13-R2). No Party/Tour/Pricing FK. No Offer.
/// </summary>
public sealed class AgencyMarketplaceDbContext : DbContext
{
    public const string SchemaName = "agency_marketplace";

    public AgencyMarketplaceDbContext(DbContextOptions<AgencyMarketplaceDbContext> options)
        : base(options)
    {
    }

    public DbSet<AgencyProfile> AgencyProfiles => Set<AgencyProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgencyMarketplaceDbContext).Assembly);
    }
}

using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.AgencyMarketplace.Domain;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure;

/// <summary>
/// Agency Marketplace-owned DbContext. Owns PostgreSQL schema <c>agency_marketplace</c>.
/// Maps AgencyProfile (P13-R2) and AgencyOffer (P13-R3). No Party/Tour/Pricing FK.
/// </summary>
public sealed class AgencyMarketplaceDbContext : DbContext
{
    public const string SchemaName = "agency_marketplace";

    public AgencyMarketplaceDbContext(DbContextOptions<AgencyMarketplaceDbContext> options)
        : base(options)
    {
    }

    public DbSet<AgencyProfile> AgencyProfiles => Set<AgencyProfile>();

    public DbSet<AgencyOffer> AgencyOffers => Set<AgencyOffer>();

    public DbSet<AgencyOfferGovernanceEvent> AgencyOfferGovernanceEvents => Set<AgencyOfferGovernanceEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgencyMarketplaceDbContext).Assembly);
    }
}

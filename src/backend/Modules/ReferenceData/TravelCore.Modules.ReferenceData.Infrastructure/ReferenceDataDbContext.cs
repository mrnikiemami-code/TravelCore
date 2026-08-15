using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.ReferenceData.Domain;

namespace TravelCore.Modules.ReferenceData.Infrastructure;

/// <summary>
/// ReferenceData-owned DbContext. Owns PostgreSQL schema <c>reference_data</c>.
/// </summary>
public sealed class ReferenceDataDbContext : DbContext
{
    public const string SchemaName = "reference_data";

    public ReferenceDataDbContext(DbContextOptions<ReferenceDataDbContext> options)
        : base(options)
    {
    }

    public DbSet<CurrencyCatalogEntry> Currencies => Set<CurrencyCatalogEntry>();

    public DbSet<LocaleCatalogEntry> Locales => Set<LocaleCatalogEntry>();

    public DbSet<CountryCatalogEntry> Countries => Set<CountryCatalogEntry>();

    public DbSet<TimeZoneCatalogEntry> TimeZones => Set<TimeZoneCatalogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReferenceDataDbContext).Assembly);
    }
}

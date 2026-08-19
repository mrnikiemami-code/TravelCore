using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.DynamicPackage.Domain;

namespace TravelCore.Modules.DynamicPackage.Infrastructure;

/// <summary>
/// DynamicPackage-owned DbContext. Owns PostgreSQL schema <c>dynamic_package</c>.
/// T001 is schema foundation only — no product tables.
/// </summary>
public sealed class DynamicPackageDbContext : DbContext
{
    public const string SchemaName = "dynamic_package";

    public DynamicPackageDbContext(DbContextOptions<DynamicPackageDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DynamicPackageDbContext).Assembly);
    }

    // P23-R2: composition boundary persistence only (no booking lifecycle / payment / orchestration).
    public DbSet<PackageComposition> PackageCompositions => Set<PackageComposition>();
}

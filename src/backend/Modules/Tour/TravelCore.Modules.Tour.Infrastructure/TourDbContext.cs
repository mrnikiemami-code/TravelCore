using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Tour.Domain;

namespace TravelCore.Modules.Tour.Infrastructure;

/// <summary>
/// Tour-owned DbContext. Owns PostgreSQL schema <c>tour</c>.
/// </summary>
public sealed class TourDbContext : DbContext
{
    public const string SchemaName = "tour";

    public TourDbContext(DbContextOptions<TourDbContext> options)
        : base(options)
    {
    }

    public DbSet<TourProduct> TourProducts => Set<TourProduct>();

    public DbSet<TourExperienceSpecialization> ExperienceSpecializations => Set<TourExperienceSpecialization>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TourDbContext).Assembly);
    }
}

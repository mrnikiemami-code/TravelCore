using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.ReferenceData.Infrastructure;

/// <summary>
/// Empty ReferenceData-owned DbContext shell. Owns PostgreSQL schema <c>reference_data</c>.
/// No entities, migrations, or business features in TC-P04-T001.
/// </summary>
public sealed class ReferenceDataDbContext : DbContext
{
    public const string SchemaName = "reference_data";

    public ReferenceDataDbContext(DbContextOptions<ReferenceDataDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
    }
}

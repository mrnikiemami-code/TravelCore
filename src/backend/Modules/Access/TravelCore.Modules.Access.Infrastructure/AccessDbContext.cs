using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Access.Infrastructure;

/// <summary>
/// Empty Access-owned DbContext shell. Owns PostgreSQL schema <c>access</c>.
/// No entities, migrations, or business features in TC-P03-T001.
/// </summary>
public sealed class AccessDbContext : DbContext
{
    public const string SchemaName = "access";

    public AccessDbContext(DbContextOptions<AccessDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
    }
}

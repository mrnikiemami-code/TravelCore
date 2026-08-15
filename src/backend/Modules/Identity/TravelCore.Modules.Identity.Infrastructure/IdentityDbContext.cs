using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Identity.Infrastructure;

/// <summary>
/// Empty Identity-owned DbContext shell. Owns PostgreSQL schema <c>identity</c>.
/// No entities, migrations, or business features in TC-P03-T001.
/// </summary>
public sealed class IdentityDbContext : DbContext
{
    public const string SchemaName = "identity";

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
    }
}

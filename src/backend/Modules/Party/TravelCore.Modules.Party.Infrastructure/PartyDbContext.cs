using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Party.Infrastructure;

/// <summary>
/// Empty Party-owned DbContext shell. Owns PostgreSQL schema <c>party</c>.
/// No entities, migrations, or business features in TC-P03-T001.
/// </summary>
public sealed class PartyDbContext : DbContext
{
    public const string SchemaName = "party";

    public PartyDbContext(DbContextOptions<PartyDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
    }
}

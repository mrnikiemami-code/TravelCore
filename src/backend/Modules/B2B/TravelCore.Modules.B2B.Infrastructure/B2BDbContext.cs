using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.B2B.Infrastructure;

/// <summary>
/// B2B-owned DbContext. Owns PostgreSQL schema <c>b2b</c>.
/// T001 is schema foundation only — no product tables.
/// </summary>
public sealed class B2BDbContext : DbContext
{
    public const string SchemaName = "b2b";

    public B2BDbContext(DbContextOptions<B2BDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
    }
}

using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Search.Infrastructure;

/// <summary>
/// Search-owned DbContext. Owns PostgreSQL schema <c>search</c>.
/// No projection tables / FTS indexes in T001 (P15-R1 scaffolding only).
/// </summary>
public sealed class SearchDbContext : DbContext
{
    public const string SchemaName = "search";

    public SearchDbContext(DbContextOptions<SearchDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SearchDbContext).Assembly);
    }
}

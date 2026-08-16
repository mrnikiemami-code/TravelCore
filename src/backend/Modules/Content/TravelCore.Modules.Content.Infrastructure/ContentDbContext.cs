using Microsoft.EntityFrameworkCore;
using ContentItemAggregate = TravelCore.Modules.Content.Domain.ContentItem;

namespace TravelCore.Modules.Content.Infrastructure;

/// <summary>
/// Content-owned DbContext. Owns PostgreSQL schema <c>content</c>.
/// </summary>
public sealed class ContentDbContext : DbContext
{
    public const string SchemaName = "content";

    public ContentDbContext(DbContextOptions<ContentDbContext> options)
        : base(options)
    {
    }

    public DbSet<ContentItemAggregate> ContentItems => Set<ContentItemAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContentDbContext).Assembly);
    }
}

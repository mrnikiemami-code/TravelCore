using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Content.Domain;
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

    public DbSet<ContentCategory> ContentCategories => Set<ContentCategory>();

    public DbSet<ContentTag> ContentTags => Set<ContentTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContentDbContext).Assembly);
        RemoveStrongIdShadowForeignKeys(modelBuilder);
    }

    /// <summary>
    /// EF can emit a duplicate shadow FK (e.g. ContentItemId1) for strong-typed id FKs on
    /// aggregates that also expose an Id-typed primary key. Keep the real ContentItemId FK only.
    /// </summary>
    private static void RemoveStrongIdShadowForeignKeys(ModelBuilder modelBuilder)
    {
        var block = modelBuilder.Entity<ContentBlock>().Metadata;
        foreach (var fk in block.GetForeignKeys()
                     .Where(x => x.Properties.Any(p => p.Name == "ContentItemId1"))
                     .ToList())
        {
            block.RemoveForeignKey(fk);
        }

        var shadow = block.FindProperty("ContentItemId1");
        if (shadow is not null)
        {
            block.RemoveProperty(shadow);
        }
    }
}

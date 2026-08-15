using Microsoft.EntityFrameworkCore;
using AccountAggregate = TravelCore.Modules.Identity.Domain.Account;

namespace TravelCore.Modules.Identity.Infrastructure;

/// <summary>
/// Identity-owned DbContext. Owns PostgreSQL schema <c>identity</c>.
/// </summary>
public sealed class IdentityDbContext : DbContext
{
    public const string SchemaName = "identity";

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<AccountAggregate> Accounts => Set<AccountAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
    }
}

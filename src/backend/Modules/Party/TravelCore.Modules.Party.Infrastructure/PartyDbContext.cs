using Microsoft.EntityFrameworkCore;
using PartyAggregate = TravelCore.Modules.Party.Domain.Party;

namespace TravelCore.Modules.Party.Infrastructure;

/// <summary>
/// Party-owned DbContext. Owns PostgreSQL schema <c>party</c>.
/// </summary>
public sealed class PartyDbContext : DbContext
{
    public const string SchemaName = "party";

    public PartyDbContext(DbContextOptions<PartyDbContext> options)
        : base(options)
    {
    }

    public DbSet<PartyAggregate> Parties => Set<PartyAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PartyDbContext).Assembly);
    }
}

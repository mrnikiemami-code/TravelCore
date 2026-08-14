using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace TravelCore.PersistenceFixture;

/// <summary>
/// Non-production fixture DbContext proving one-module ownership of DbContext + PostgreSQL schema.
/// Not a business module; must never be registered in TravelCore.Api.
/// </summary>
public sealed class PersistenceFixtureDbContext : DbContext
{
    public const string SchemaName = "p01_fixture";

    public PersistenceFixtureDbContext(DbContextOptions<PersistenceFixtureDbContext> options)
        : base(options)
    {
    }

    internal DbSet<PersistenceProbe> PersistenceProbes => Set<PersistenceProbe>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        // مالکیت schema با fixture است، نه با TravelCore.Persistence.PostgreSql.
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.Entity<PersistenceProbe>(entity =>
        {
            entity.ToTable("persistence_probes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.InstantValue);
            entity.Property(x => x.LocalDateValue);
            entity.Property(x => x.LocalTimeValue);
            entity.Property(x => x.LocalDateTimeValue);
        });
    }
}

/// <summary>
/// Technical persistence probe only — not a domain entity.
/// </summary>
internal sealed class PersistenceProbe
{
    public Guid Id { get; set; }

    public Instant InstantValue { get; set; }

    public LocalDate LocalDateValue { get; set; }

    public LocalTime LocalTimeValue { get; set; }

    public LocalDateTime LocalDateTimeValue { get; set; }
}

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

    internal DbSet<PersistenceFixtureOutboxMessage> OutboxMessages => Set<PersistenceFixtureOutboxMessage>();

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

        modelBuilder.Entity<PersistenceFixtureOutboxMessage>(entity =>
        {
            // Outbox محلی همان مالک DbContext/schema — نه جدول سراسری.
            entity.ToTable("outbox_messages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OccurredAt).IsRequired();
            entity.Property(x => x.MessageType).IsRequired().HasMaxLength(256);
            entity.Property(x => x.Payload).IsRequired().HasColumnType("jsonb");
            entity.Property(x => x.ProcessedAt);
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

/// <summary>
/// Technical fixture-owned Outbox row — not a business aggregate.
/// Proves module-local transactional Outbox persistence shape only (no dispatch).
/// </summary>
internal sealed class PersistenceFixtureOutboxMessage
{
    public Guid Id { get; set; }

    public Instant OccurredAt { get; set; }

    public string MessageType { get; set; } = null!;

    public string Payload { get; set; } = null!;

    public Instant? ProcessedAt { get; set; }
}

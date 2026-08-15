using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.PersistenceFixture;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Serialize this suite: one shared ephemeral PostgreSQL database.
/// </summary>
[CollectionDefinition(nameof(PostgreSqlIntegrationCollection), DisableParallelization = true)]
public sealed class PostgreSqlIntegrationCollection : ICollectionFixture<PostgreSqlContainerFixture>;

[Collection(nameof(PostgreSqlIntegrationCollection))]
public sealed class PersistencePostgreSqlIntegrationTests
{
    private readonly PostgreSqlContainerFixture _postgres;

    public PersistencePostgreSqlIntegrationTests(PostgreSqlContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task RealPostgreSql_Connectivity_Schema_And_Version()
    {
        await using var db = _postgres.CreateDbContext();
        Assert.Contains("npgsql", db.Database.ProviderName ?? string.Empty, StringComparison.OrdinalIgnoreCase);

        var conn = db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync(TestContext.Current.CancellationToken);
        Assert.Equal(System.Data.ConnectionState.Open, conn.State);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SHOW server_version;";
        var serverVersion = (string?)await cmd.ExecuteScalarAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(serverVersion);
        Assert.StartsWith("18.6", serverVersion, StringComparison.Ordinal);

        cmd.CommandText = """
            SELECT nspname
            FROM pg_namespace
            WHERE nspname = 'p01_fixture';
            """;
        var schema = (string?)await cmd.ExecuteScalarAsync(TestContext.Current.CancellationToken);
        Assert.Equal("p01_fixture", schema);

        cmd.CommandText = """
            SELECT COUNT(*)::int
            FROM information_schema.tables
            WHERE table_schema = 'p01_fixture'
              AND table_name IN ('persistence_probes', 'outbox_messages', '__EFMigrationsHistory');
            """;
        var tables = (int)(await cmd.ExecuteScalarAsync(TestContext.Current.CancellationToken) ?? 0);
        Assert.Equal(3, tables);

        // Fixture-owned objects must not accidentally land in public.
        cmd.CommandText = """
            SELECT COUNT(*)::int
            FROM information_schema.tables
            WHERE table_schema = 'public'
              AND table_name IN ('persistence_probes', 'outbox_messages');
            """;
        var publicDupes = (int)(await cmd.ExecuteScalarAsync(TestContext.Current.CancellationToken) ?? 0);
        Assert.Equal(0, publicDupes);

        await db.Database.CloseConnectionAsync();
        Assert.Equal(System.Data.ConnectionState.Closed, conn.State);
    }

    [Fact]
    public async Task PersistenceProbe_NodaTime_RoundTrips_WithStoreTypes()
    {
        var id = Guid.CreateVersion7();
        var instant = Instant.FromUtc(2026, 8, 15, 0, 30, 0);
        var localDate = new LocalDate(2026, 8, 15);
        var localTime = new LocalTime(13, 45, 30);
        var localDateTime = new LocalDateTime(2026, 8, 15, 13, 45, 30);

        await using (var db = _postgres.CreateDbContext())
        {
            db.PersistenceProbes.Add(new PersistenceProbe
            {
                Id = id,
                InstantValue = instant,
                LocalDateValue = localDate,
                LocalTimeValue = localTime,
                LocalDateTimeValue = localDateTime
            });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var loaded = await db.PersistenceProbes.SingleAsync(x => x.Id == id, TestContext.Current.CancellationToken);
            Assert.Equal(instant, loaded.InstantValue);
            Assert.Equal(localDate, loaded.LocalDateValue);
            Assert.Equal(localTime, loaded.LocalTimeValue);
            Assert.Equal(localDateTime, loaded.LocalDateTimeValue);

            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(TestContext.Current.CancellationToken);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT
                  pg_typeof("InstantValue")::text,
                  pg_typeof("LocalDateValue")::text,
                  pg_typeof("LocalTimeValue")::text,
                  pg_typeof("LocalDateTimeValue")::text
                FROM p01_fixture.persistence_probes
                WHERE "Id" = @id;
                """;
            var p = cmd.CreateParameter();
            p.ParameterName = "id";
            p.Value = id;
            cmd.Parameters.Add(p);
            await using var reader = await cmd.ExecuteReaderAsync(TestContext.Current.CancellationToken);
            Assert.True(await reader.ReadAsync(TestContext.Current.CancellationToken));
            Assert.Equal("timestamp with time zone", reader.GetString(0));
            Assert.Equal("date", reader.GetString(1));
            Assert.Equal("time without time zone", reader.GetString(2));
            Assert.Equal("timestamp without time zone", reader.GetString(3));
        }
    }

    [Fact]
    public async Task Outbox_JsonbPayload_RoundTrips_InFixtureSchema()
    {
        var id = Guid.CreateVersion7();
        var occurred = Instant.FromUtc(2026, 8, 15, 1, 0, 0);
        const string messageType = "travelcore.fixture.persistence-probe-recorded.v1";
        const string payload = """{"probeId":"demo","ok":true}""";

        await using (var db = _postgres.CreateDbContext())
        {
            db.OutboxMessages.Add(new PersistenceFixtureOutboxMessage
            {
                Id = id,
                OccurredAt = occurred,
                MessageType = messageType,
                Payload = payload,
                ProcessedAt = null
            });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var loaded = await db.OutboxMessages.SingleAsync(x => x.Id == id, TestContext.Current.CancellationToken);
            Assert.Equal(occurred, loaded.OccurredAt);
            Assert.Equal(messageType, loaded.MessageType);
            // jsonb may canonicalize key order / whitespace; assert semantic JSON, not byte-identical text.
            using (var expectedDoc = System.Text.Json.JsonDocument.Parse(payload))
            using (var actualDoc = System.Text.Json.JsonDocument.Parse(loaded.Payload))
            {
                Assert.True(System.Text.Json.JsonElement.DeepEquals(expectedDoc.RootElement, actualDoc.RootElement));
            }
            Assert.Null(loaded.ProcessedAt);

            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(TestContext.Current.CancellationToken);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT pg_typeof("Payload")::text
                FROM p01_fixture.outbox_messages
                WHERE "Id" = @id;
                """;
            var p = cmd.CreateParameter();
            p.ParameterName = "id";
            p.Value = id;
            cmd.Parameters.Add(p);
            var typeName = (string?)await cmd.ExecuteScalarAsync(TestContext.Current.CancellationToken);
            Assert.Equal("jsonb", typeName);
        }
    }

    [Fact]
    public async Task SameDbContext_SaveChanges_PersistsProbeAndOutbox()
    {
        var probeId = Guid.CreateVersion7();
        var outboxId = Guid.CreateVersion7();

        await using (var db = _postgres.CreateDbContext())
        {
            db.PersistenceProbes.Add(CreateProbe(probeId));
            db.OutboxMessages.Add(CreateOutbox(outboxId, probeId));
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            Assert.True(await db.PersistenceProbes.AnyAsync(x => x.Id == probeId, TestContext.Current.CancellationToken));
            Assert.True(await db.OutboxMessages.AnyAsync(x => x.Id == outboxId, TestContext.Current.CancellationToken));
        }
    }

    [Fact]
    public async Task ModuleState_And_Outbox_CommitTogether()
    {
        var probeId = Guid.CreateVersion7();
        var outboxId = Guid.CreateVersion7();

        await using (var db = _postgres.CreateDbContext())
        {
            await using var tx = await db.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
            db.PersistenceProbes.Add(CreateProbe(probeId));
            db.OutboxMessages.Add(CreateOutbox(outboxId, probeId));
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            await tx.CommitAsync(TestContext.Current.CancellationToken);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            Assert.True(await db.PersistenceProbes.AnyAsync(x => x.Id == probeId, TestContext.Current.CancellationToken));
            Assert.True(await db.OutboxMessages.AnyAsync(x => x.Id == outboxId, TestContext.Current.CancellationToken));
        }
    }

    [Fact]
    public async Task ModuleState_And_Outbox_RollbackTogether()
    {
        var probeId = Guid.CreateVersion7();
        var outboxId = Guid.CreateVersion7();

        await using (var db = _postgres.CreateDbContext())
        {
            await using var tx = await db.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
            db.PersistenceProbes.Add(CreateProbe(probeId));
            db.OutboxMessages.Add(CreateOutbox(outboxId, probeId));
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            await tx.RollbackAsync(TestContext.Current.CancellationToken);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            Assert.False(await db.PersistenceProbes.AnyAsync(x => x.Id == probeId, TestContext.Current.CancellationToken));
            Assert.False(await db.OutboxMessages.AnyAsync(x => x.Id == outboxId, TestContext.Current.CancellationToken));
        }
    }

    private static PersistenceProbe CreateProbe(Guid id) => new()
    {
        Id = id,
        InstantValue = Instant.FromUtc(2026, 1, 1, 12, 30, 45),
        LocalDateValue = new LocalDate(2026, 3, 15),
        LocalTimeValue = new LocalTime(9, 15, 0),
        LocalDateTimeValue = new LocalDateTime(2026, 3, 15, 9, 15, 0)
    };

    private static PersistenceFixtureOutboxMessage CreateOutbox(Guid id, Guid probeId) => new()
    {
        Id = id,
        OccurredAt = Instant.FromUtc(2026, 1, 1, 12, 30, 46),
        MessageType = "travelcore.fixture.persistence-probe-recorded.v1",
        Payload = $"{{\"probeId\":\"{probeId}\"}}",
        ProcessedAt = null
    };
}

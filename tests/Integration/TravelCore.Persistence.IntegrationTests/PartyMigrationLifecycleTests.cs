using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Party.Infrastructure;
using PartyAggregate = TravelCore.Modules.Party.Domain.Party;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL Party migration + CRUD smoke (TC-P03-T002).
/// </summary>
[Collection(nameof(PartyMigrationLifecycleCollection))]
public sealed class PartyMigrationLifecycleTests
{
    private readonly PartyMigrationLifecycleContainerFixture _postgres;

    public PartyMigrationLifecycleTests(PartyMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task PartyMigrationLifecycle_CleanDatabase_Apply_NoOp_And_CrudSmoke()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Single(expectedMigrations);
            Assert.EndsWith("_InitialPartyPersistence", expectedMigrations[0], StringComparison.Ordinal);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'party';
                """, ct));

            var pendingBefore = (await db.Database.GetPendingMigrationsAsync(ct)).ToArray();
            Assert.Equal(expectedMigrations, pendingBefore);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await PartyMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'party';
                """, ct));
            Assert.Equal(5, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'party'
                  AND table_name IN (
                    'parties',
                    'party_persons',
                    'party_organizations',
                    'party_agencies',
                    '__EFMigrationsHistory');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'public'
                  AND table_name IN (
                    'parties',
                    'party_persons',
                    'party_organizations',
                    'party_agencies',
                    '__EFMigrationsHistory');
                """, ct));

            var historyIds = await ReadHistoryIdsAsync(conn, ct);
            Assert.Equal(expectedMigrations, historyIds);
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.False(db.Database.HasPendingModelChanges());

            await AssertTemporalColumnsAsync(conn, ct);
        }

        // Second migrate no-op
        await using (var db = _postgres.CreateDbContext())
        {
            await PartyMigrator.MigrateAsync(db, ct);
        }

        // CRUD smoke across specializations
        var now = Instant.FromUtc(2026, 8, 15, 13, 0);
        await using (var db = _postgres.CreateDbContext())
        {
            db.Parties.Add(PartyAggregate.CreatePerson("Ada Lovelace", "Ada", "Lovelace", now));
            db.Parties.Add(PartyAggregate.CreateOrganization("Acme Org", "Acme Legal", now));
            db.Parties.Add(PartyAggregate.CreateAgency("Sky Agency", "Sky Trading", now, "LIC-9"));
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var count = await db.Parties.CountAsync(ct);
            Assert.Equal(3, count);
            Assert.Equal(1, await db.Parties.CountAsync(x => x.Person != null, ct));
            Assert.Equal(1, await db.Parties.CountAsync(x => x.Organization != null, ct));
            Assert.Equal(1, await db.Parties.CountAsync(x => x.Agency != null, ct));
        }

        var repoRoot = FindRepoRoot();
        var snapshotPath = Path.Combine(
            repoRoot,
            "src", "backend", "Modules", "Party",
            "TravelCore.Modules.Party.Infrastructure",
            "Migrations", "PartyDbContextModelSnapshot.cs");
        Assert.True(File.Exists(snapshotPath), $"Expected snapshot at {snapshotPath}");
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "TravelCore.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("TravelCore.sln not found from test base directory.");
    }

    private static async Task AssertTemporalColumnsAsync(DbConnection conn, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT column_name, udt_name
            FROM information_schema.columns
            WHERE table_schema = 'party'
              AND table_name = 'parties'
              AND column_name IN ('created_at', 'updated_at');
            """;
        var types = new Dictionary<string, string>(StringComparer.Ordinal);
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                types[reader.GetString(0)] = reader.GetString(1);
            }
        }

        Assert.Equal("timestamptz", types["created_at"]);
        Assert.Equal("timestamptz", types["updated_at"]);
    }

    private static async Task<string[]> ReadHistoryIdsAsync(DbConnection conn, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT "MigrationId"
            FROM party."__EFMigrationsHistory"
            ORDER BY "MigrationId";
            """;
        var ids = new List<string>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            ids.Add(reader.GetString(0));
        }

        return ids.ToArray();
    }

    private static async Task<int> ScalarIntAsync(DbConnection conn, string sql, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        return (int)(await cmd.ExecuteScalarAsync(ct) ?? 0);
    }
}

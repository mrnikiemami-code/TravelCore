using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TravelCore.PersistenceFixture;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Ordered real-PostgreSQL migration acceptance (TC-P01-T017).
/// Clean DB → pending → migrate → history → second migrate no-op → type regression.
/// </summary>
[Collection(nameof(MigrationLifecycleCollection))]
public sealed class PersistencePostgreSqlMigrationLifecycleTests
{
    private static readonly string[] ExpectedMigrations =
    [
        "20260814235910_InitialPersistenceFixture",
        "20260815000315_AddModuleLocalOutbox"
    ];

    private readonly MigrationLifecycleContainerFixture _postgres;

    public PersistencePostgreSqlMigrationLifecycleTests(MigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task MigrationLifecycle_CleanDatabase_Apply_NoOp_And_SchemaProof()
    {
        var ct = TestContext.Current.CancellationToken;

        // --- Compiled migration inventory (EF metadata, not filenames alone) ---
        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            var defined = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Equal(2, defined.Length);
            Assert.Equal(ExpectedMigrations, defined);
        }

        // --- Clean initial catalog state ---
        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'p01_fixture';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'p01_fixture'
                  AND table_name = '__EFMigrationsHistory';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_name IN ('persistence_probes', 'outbox_messages');
                """, ct));

            var pendingBefore = (await db.Database.GetPendingMigrationsAsync(ct)).ToArray();
            var appliedBefore = (await db.Database.GetAppliedMigrationsAsync(ct)).ToArray();
            Assert.Equal(ExpectedMigrations, pendingBefore);
            Assert.Empty(appliedBefore);
        }

        // --- First apply via module-owned migrator ---
        await using (var db = _postgres.CreateDbContext())
        {
            await PersistenceFixtureMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'p01_fixture';
                """, ct));
            Assert.Equal(3, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'p01_fixture'
                  AND table_name IN ('persistence_probes', 'outbox_messages', '__EFMigrationsHistory');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'public'
                  AND table_name IN ('persistence_probes', 'outbox_messages', '__EFMigrationsHistory');
                """, ct));

            var historyIds = await ReadHistoryIdsAsync(conn, ct);
            Assert.Equal(ExpectedMigrations, historyIds);
            Assert.Equal(historyIds.Distinct(StringComparer.Ordinal).Count(), historyIds.Length);

            var applied = (await db.Database.GetAppliedMigrationsAsync(ct)).ToArray();
            var pending = (await db.Database.GetPendingMigrationsAsync(ct)).ToArray();
            Assert.Equal(ExpectedMigrations, applied);
            Assert.Empty(pending);

            Assert.False(db.Database.HasPendingModelChanges());

            await AssertTemporalAndJsonbColumnsAsync(conn, ct);
        }

        // --- Second migrate is a safe no-op ---
        await using (var db = _postgres.CreateDbContext())
        {
            await PersistenceFixtureMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);
            var historyIds = await ReadHistoryIdsAsync(conn, ct);
            Assert.Equal(ExpectedMigrations, historyIds);
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.Equal(ExpectedMigrations, (await db.Database.GetAppliedMigrationsAsync(ct)).ToArray());
        }

        // Snapshot ownership (static path under fixture project)
        var repoRoot = FindRepoRoot();
        var snapshotPath = Path.Combine(
            repoRoot,
            "tests", "Fixtures", "Persistence", "TravelCore.PersistenceFixture",
            "Migrations", "PersistenceFixtureDbContextModelSnapshot.cs");
        Assert.True(File.Exists(snapshotPath), $"Expected snapshot at {snapshotPath}");
        Assert.DoesNotContain(
            Path.Combine("src", "backend"),
            snapshotPath,
            StringComparison.OrdinalIgnoreCase);
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

    private static async Task AssertTemporalAndJsonbColumnsAsync(DbConnection conn, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT column_name, udt_name, is_nullable
            FROM information_schema.columns
            WHERE table_schema = 'p01_fixture'
              AND table_name = 'persistence_probes'
              AND column_name IN ('InstantValue', 'LocalDateValue', 'LocalTimeValue', 'LocalDateTimeValue');
            """;
        var probeTypes = new Dictionary<string, string>(StringComparer.Ordinal);
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                probeTypes[reader.GetString(0)] = reader.GetString(1);
            }
        }

        Assert.Equal("timestamptz", probeTypes["InstantValue"]);
        Assert.Equal("date", probeTypes["LocalDateValue"]);
        Assert.Equal("time", probeTypes["LocalTimeValue"]);
        Assert.Equal("timestamp", probeTypes["LocalDateTimeValue"]);

        cmd.CommandText = """
            SELECT column_name, udt_name, is_nullable
            FROM information_schema.columns
            WHERE table_schema = 'p01_fixture'
              AND table_name = 'outbox_messages'
              AND column_name IN ('OccurredAt', 'ProcessedAt', 'Payload');
            """;
        var outbox = new Dictionary<string, (string Udt, string Nullable)>(StringComparer.Ordinal);
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                outbox[reader.GetString(0)] = (reader.GetString(1), reader.GetString(2));
            }
        }

        Assert.Equal("timestamptz", outbox["OccurredAt"].Udt);
        Assert.Equal("timestamptz", outbox["ProcessedAt"].Udt);
        Assert.Equal("YES", outbox["ProcessedAt"].Nullable);
        Assert.Equal("jsonb", outbox["Payload"].Udt);
    }

    private static async Task<string[]> ReadHistoryIdsAsync(DbConnection conn, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT "MigrationId"
            FROM p01_fixture."__EFMigrationsHistory"
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

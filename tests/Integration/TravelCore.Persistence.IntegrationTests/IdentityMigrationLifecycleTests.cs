using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Identity.Infrastructure;
using TravelCore.Modules.Identity.Infrastructure.Security;
using AccountAggregate = TravelCore.Modules.Identity.Domain.Account;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(IdentityMigrationLifecycleCollection))]
public sealed class IdentityMigrationLifecycleTests
{
    private readonly IdentityMigrationLifecycleContainerFixture _postgres;

    public IdentityMigrationLifecycleTests(IdentityMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task IdentityMigrationLifecycle_SchemaOwnership_And_CredentialPersistence()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Single(expectedMigrations);
            Assert.EndsWith("_InitialIdentityPersistence", expectedMigrations[0], StringComparison.Ordinal);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await IdentityMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'identity';
                """, ct));
            Assert.Equal(2, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'identity'
                  AND table_name IN ('accounts', '__EFMigrationsHistory');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'public'
                  AND table_name IN ('accounts', '__EFMigrationsHistory');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints
                WHERE constraint_type = 'FOREIGN KEY'
                  AND table_schema = 'identity'
                  AND table_name = 'accounts';
                """, ct));

            Assert.Equal(expectedMigrations, await ReadHistoryIdsAsync(conn, ct));
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.False(db.Database.HasPendingModelChanges());
        }

        // Second migrate no-op
        await using (var db = _postgres.CreateDbContext())
        {
            await IdentityMigrator.MigrateAsync(db, ct);
        }

        var hasher = new AspNetCoreIdentityPasswordHasher();
        var now = Instant.FromUtc(2026, 8, 15, 14, 30);
        const string password = "Identity-Test-Password-1";
        var hash = hasher.HashPassword(password);
        var partyRef = Guid.CreateVersion7();

        await using (var db = _postgres.CreateDbContext())
        {
            db.Accounts.Add(AccountAggregate.Create("ops@travelcore.test", hash, now, partyRef));
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var account = await db.Accounts.SingleAsync(ct);
            Assert.Equal("OPS@TRAVELCORE.TEST", account.NormalizedEmail);
            Assert.Equal(partyRef, account.AssociatedPartyId);
            Assert.NotEqual(password, account.PasswordHash);
            Assert.True(hasher.VerifyHashedPassword(account.PasswordHash, password));
        }
    }

    private static async Task<string[]> ReadHistoryIdsAsync(DbConnection conn, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT "MigrationId"
            FROM identity."__EFMigrationsHistory"
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

using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Testcontainers.PostgreSql;
using TravelCore.Modules.Access.Domain;
using TravelCore.Modules.Access.Infrastructure;
using TravelCore.Modules.Access.Infrastructure.Seeding;
using TravelCore.Persistence.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

public sealed class AccessMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_access_migration_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public AccessDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AccessDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: AccessDbContext.SchemaName)
            .Options;
        return new AccessDbContext(options);
    }
}

[CollectionDefinition(nameof(AccessMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class AccessMigrationLifecycleCollection : ICollectionFixture<AccessMigrationLifecycleContainerFixture>;

[Collection(nameof(AccessMigrationLifecycleCollection))]
public sealed class AccessMigrationLifecycleTests
{
    private readonly AccessMigrationLifecycleContainerFixture _postgres;

    public AccessMigrationLifecycleTests(AccessMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task AccessMigration_SchemaOwnership_And_AdminSeed()
    {
        var ct = TestContext.Current.CancellationToken;

        await using (var inventory = _postgres.CreateDbContext())
        {
            var migrations = inventory.Database.GetMigrations().ToArray();
            Assert.Single(migrations);
            Assert.EndsWith("_InitialAccessPersistence", migrations[0], StringComparison.Ordinal);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await AccessMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);
            Assert.Equal(1, await ScalarIntAsync(conn, "SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'access';", ct));
            Assert.Equal(4, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM information_schema.tables
                WHERE table_schema = 'access'
                  AND table_name IN ('permissions','roles','role_permissions','__EFMigrationsHistory');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM information_schema.tables
                WHERE table_schema = 'public'
                  AND table_name IN ('permissions','roles','role_permissions','__EFMigrationsHistory');
                """, ct));
            Assert.False(db.Database.HasPendingModelChanges());
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(db, SystemClock.Instance, ct);
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(db, SystemClock.Instance, ct); // idempotent
        }

        await using (var db = _postgres.CreateDbContext())
        {
            Assert.Equal(AccessPermissionCatalog.AdminBaseline.Count, await db.Permissions.CountAsync(ct));
            var admin = await db.Roles.SingleAsync(x => x.Code == AccessPermissionCatalog.AdminRoleCode, ct);
            Assert.Equal(AccessPermissionCatalog.AdminBaseline.Count, admin.Permissions.Count);
        }
    }

    private static async Task<int> ScalarIntAsync(DbConnection conn, string sql, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        return (int)(await cmd.ExecuteScalarAsync(ct) ?? 0);
    }
}

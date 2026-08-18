using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Payment.Infrastructure;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL Payment schema scaffolding smoke (TC-P20-T001).
/// </summary>
[Collection(nameof(PaymentMigrationLifecycleCollection))]
public sealed class PaymentMigrationLifecycleTests
{
    private readonly PaymentMigrationLifecycleContainerFixture _postgres;

    public PaymentMigrationLifecycleTests(PaymentMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task PaymentMigrationLifecycle_Apply_EnsureSchema_Only()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Equal(6, expectedMigrations.Length);
            Assert.EndsWith("_InitialPaymentScaffolding", expectedMigrations[0], StringComparison.Ordinal);
            Assert.EndsWith("_AddPaymentAggregateBaseline", expectedMigrations[1], StringComparison.Ordinal);
            Assert.EndsWith("_AddPaymentAttemptProviderReferences", expectedMigrations[2], StringComparison.Ordinal);
            Assert.EndsWith("_AddPaymentIdempotencyAndReconciliation", expectedMigrations[3], StringComparison.Ordinal);
            Assert.EndsWith("_AddPaymentExecutionSnapshotAndAmountVerification", expectedMigrations[4], StringComparison.Ordinal);
            Assert.EndsWith("_AddPaymentSuccessOutbox", expectedMigrations[5], StringComparison.Ordinal);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await PaymentMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'payment';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'payment'
                  AND table_name = '__EFMigrationsHistory';
                """, ct));
            Assert.Equal(5, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'payment'
                  AND table_name NOT IN ('__EFMigrationsHistory');
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'payment'
                  AND table_name = 'payments';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'payment'
                  AND table_name = 'payments'
                  AND column_name = 'version';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'payment'
                  AND table_name = 'payment_attempts';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'payment'
                  AND table_name = 'payment_initiation_idempotency';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'payment'
                  AND table_name = 'payment_reconciliation_issues';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'payment'
                  AND table_name = 'outbox_messages';
                """, ct));
            Assert.Equal(3, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'payment'
                  AND table_name = 'payment_attempts'
                  AND column_name IN (
                        'provider_key',
                        'provider_request_reference',
                        'provider_transaction_reference');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'payment'
                  AND table_name IN (
                        'payment_transactions',
                        'refunds', 'provider_callbacks', 'settlements', 'wallets');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.table_schema = 'payment'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema IN (
                        'booking', 'tour', 'pricing', 'party', 'identity',
                        'agency_marketplace', 'search', 'visa', 'trip_planner');
                """, ct));
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.Equal("payment", db.Model.GetDefaultSchema());
        }
    }

    private static async Task<int> ScalarIntAsync(
        DbConnection conn,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }
}

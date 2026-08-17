using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.AgencyMarketplace.Infrastructure;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL Agency Marketplace schema + AgencyProfile + AgencyOffer + commercial terms (TC-P13-T001..T004).
/// </summary>
[Collection(nameof(AgencyMarketplaceMigrationLifecycleCollection))]
public sealed class AgencyMarketplaceMigrationLifecycleTests
{
    private readonly AgencyMarketplaceMigrationLifecycleContainerFixture _postgres;

    public AgencyMarketplaceMigrationLifecycleTests(AgencyMarketplaceMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task AgencyMarketplaceMigrationLifecycle_Apply_Creates_AgencyProfile_Without_Peer_Fk()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Equal(5, expectedMigrations.Length);
            Assert.Contains(
                expectedMigrations,
                m => m.EndsWith("_InitialAgencyMarketplaceScaffolding", StringComparison.Ordinal));
            Assert.Contains(
                expectedMigrations,
                m => m.EndsWith("_AddAgencyProfile", StringComparison.Ordinal));
            Assert.Contains(
                expectedMigrations,
                m => m.EndsWith("_AddAgencyOffer", StringComparison.Ordinal));
            Assert.Contains(
                expectedMigrations,
                m => m.EndsWith("_AddAgencyOfferCommercialBoundary", StringComparison.Ordinal));
            Assert.Contains(
                expectedMigrations,
                m => m.EndsWith("_AddAgencyOfferCapacityBoundary", StringComparison.Ordinal));
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await AgencyMarketplaceMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'agency_marketplace';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'agency_marketplace'
                  AND table_name = '__EFMigrationsHistory';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'agency_marketplace'
                  AND table_name IN ('commercial_settings');
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'agency_marketplace'
                  AND table_name = 'agency_offers';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'agency_marketplace'
                  AND table_name = 'agency_profiles';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'agency_marketplace'
                  AND table_name = 'agency_profiles'
                  AND column_name = 'party_id';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'agency_marketplace'
                  AND table_name = 'agency_offers'
                  AND column_name IN ('price_id', 'quote_id', 'commission_rate', 'departure_id', 'amount', 'currency_code', 'discount', 'price_override', 'available_seats', 'reserved_seats', 'capacity');
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'agency_marketplace'
                  AND table_name = 'agency_offers'
                  AND column_name = 'sales_open';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'agency_marketplace'
                  AND table_name = 'agency_offers'
                  AND column_name = 'referenced_tour_departure_id';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'agency_marketplace'
                  AND table_name = 'agency_offers'
                  AND column_name = 'requires_manual_confirmation';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'agency_marketplace'
                  AND table_name = 'agency_offers'
                  AND column_name = 'exclusive_listing';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.table_schema = 'agency_marketplace'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema IN ('party', 'tour', 'pricing');
                """, ct));
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.Equal("agency_marketplace", db.Model.GetDefaultSchema());
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

using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Visa.Infrastructure;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL Visa Definition/RequirementSet lifecycle (TC-P17-T002).
/// </summary>
[Collection(nameof(VisaMigrationLifecycleCollection))]
public sealed class VisaMigrationLifecycleTests
{
    private readonly VisaMigrationLifecycleContainerFixture _postgres;

    public VisaMigrationLifecycleTests(VisaMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task VisaMigrationLifecycle_Apply_EnsureSchema_Only()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Equal(6, expectedMigrations.Length);
            Assert.EndsWith("_InitialVisaScaffolding", expectedMigrations[0], StringComparison.Ordinal);
            Assert.EndsWith("_AddVisaDefinitionBaseline", expectedMigrations[1], StringComparison.Ordinal);
            Assert.EndsWith("_AddVisaApplicabilityBaseline", expectedMigrations[2], StringComparison.Ordinal);
            Assert.EndsWith("_AddVisaRequirementFactsBaseline", expectedMigrations[3], StringComparison.Ordinal);
            Assert.EndsWith("_AddVisaProcessingValidityBaseline", expectedMigrations[4], StringComparison.Ordinal);
            Assert.EndsWith("_AddVisaOfficialFeeBaseline", expectedMigrations[5], StringComparison.Ordinal);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await VisaMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'visa';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'visa'
                  AND table_name = '__EFMigrationsHistory';
                """, ct));
            Assert.Equal(13, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'visa'
                  AND table_name NOT IN ('__EFMigrationsHistory');
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'visa'
                  AND table_name = 'visa_definitions';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'visa'
                  AND table_name = 'visa_definition_translations';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'visa'
                  AND table_name = 'visa_requirement_sets';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'visa'
                  AND table_name = 'visa_applicabilities';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'visa'
                  AND table_name = 'visa_required_documents';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'visa'
                  AND table_name = 'visa_eligibility_requirements';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'visa'
                  AND table_name = 'visa_processing_times';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'visa'
                  AND table_name = 'visa_validities';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'visa'
                  AND table_name = 'visa_allowed_stays';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'visa'
                  AND table_name = 'visa_entry_policies';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'visa'
                  AND table_name = 'visa_official_fees';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'visa'
                  AND table_name IN ('visa_requirements', 'required_documents', 'visa_fees', 'visa_applications', 'countries', 'destinations');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'visa'
                  AND column_name IN ('price', 'quote', 'discount', 'commission', 'markup', 'duration');
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'visa'
                  AND table_name = 'visa_official_fees'
                  AND column_name = 'amount';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.table_schema = 'visa'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema IN ('identity', 'party', 'tour', 'place', 'destination', 'reference_data', 'content', 'media', 'seo', 'search', 'pricing', 'ugc');
                """, ct));
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.Equal("visa", db.Model.GetDefaultSchema());
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

using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using TravelCore.Modules.Payment.Infrastructure;
using Xunit;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(PaymentMigrationLifecycleCollection))]
public sealed class PaymentAggregatePersistenceTests
{
    private readonly PaymentMigrationLifecycleContainerFixture _postgres;

    public PaymentAggregatePersistenceTests(PaymentMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Payment_And_Attempts_RoundTrip_Without_Peer_Fk_Or_Provider_Columns()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await PaymentMigrator.MigrateAsync(migrate, ct);
        }

        var booking = new BookingReference(Guid.Parse("0198b3e0-0000-7000-8000-000000000221"));
        var createdAt = Instant.FromUtc(2026, 8, 18, 7, 0);
        PaymentId id;
        PaymentAttemptId firstAttemptId;
        PaymentAttemptId secondAttemptId;

        await using (var db = _postgres.CreateDbContext())
        {
            var payment = PaymentAggregate.Create(booking, createdAt);
            var first = payment.CreateAttempt(createdAt);
            payment.InitiateAttempt(first.Id, Instant.FromUtc(2026, 8, 18, 7, 5));
            payment.RecordAttemptFailure(first.Id, Instant.FromUtc(2026, 8, 18, 7, 10));
            var second = payment.CreateAttempt(Instant.FromUtc(2026, 8, 18, 7, 15));
            id = payment.Id;
            firstAttemptId = first.Id;
            secondAttemptId = second.Id;
            db.Payments.Add(payment);
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var loaded = await db.Payments
                .Include(x => x.Attempts)
                .SingleAsync(x => x.Id == id, ct);
            Assert.Equal(PaymentStatus.Pending, loaded.Status);
            Assert.Equal(booking, loaded.Booking);
            Assert.Equal(2, loaded.Attempts.Count);
            Assert.Equal(PaymentAttemptStatus.Failed, loaded.Attempts.Single(a => a.Id.Equals(firstAttemptId)).Status);
            Assert.Equal(PaymentAttemptStatus.Created, loaded.Attempts.Single(a => a.Id.Equals(secondAttemptId)).Status);

            loaded.RecordAuthoritativeCollectionSuccess(secondAttemptId, Instant.FromUtc(2026, 8, 18, 7, 20));
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var loaded = await db.Payments
                .Include(x => x.Attempts)
                .SingleAsync(x => x.Id == id, ct);
            Assert.Equal(PaymentStatus.Succeeded, loaded.Status);
            Assert.Equal(PaymentAttemptStatus.Succeeded, loaded.Attempts.Single(a => a.Id.Equals(secondAttemptId)).Status);
            Assert.Equal(1, loaded.Attempts.Count(a => a.Status == PaymentAttemptStatus.Succeeded));

            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'payment'
                  AND table_name = 'payments'
                  AND column_name IN ('id', 'booking_id', 'status', 'created_at', 'status_changed_at');
                """;
            Assert.Equal(5, Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)));

            cmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'payment'
                  AND table_name = 'payments'
                  AND column_name IN (
                        'provider_transaction_id', 'stripe_id', 'zarinpal_id',
                        'refund_id', 'is_paid', 'payment_succeeded');
                """;
            Assert.Equal(0, Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)));

            cmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'payment'
                  AND table_name IN ('refunds', 'provider_callbacks', 'settlements', 'wallets');
                """;
            Assert.Equal(0, Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)));

            cmd.CommandText = """
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
                """;
            Assert.Equal(0, Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)));

            cmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.table_schema = 'payment'
                  AND tc.table_name = 'payment_attempts'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema = 'payment'
                  AND ccu.table_name = 'payments';
                """;
            Assert.Equal(1, Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)));
        }
    }
}

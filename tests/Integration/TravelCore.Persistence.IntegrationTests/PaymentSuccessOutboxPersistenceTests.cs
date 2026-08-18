using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using TravelCore.Modules.Payment.Infrastructure;
using TravelCore.Modules.Payment.Infrastructure.Services;
using Xunit;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(PaymentMigrationLifecycleCollection))]
public sealed class PaymentSuccessOutboxPersistenceTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 19, 0);
    private readonly PaymentMigrationLifecycleContainerFixture _postgres;

    public PaymentSuccessOutboxPersistenceTests(PaymentMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Payment_Success_And_Outbox_Commit_Atomically()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await PaymentMigrator.MigrateAsync(migrate, ct);
        }

        PaymentId paymentId;
        await using (var db = _postgres.CreateDbContext())
        {
            var payment = CreateInitiated();
            paymentId = payment.Id;
            db.Payments.Add(payment);
            await db.SaveChangesAsync(ct);
            ApplySuccess(db, payment, Now.Plus(Duration.FromMinutes(2)));
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var payment = await db.Payments.SingleAsync(x => x.Id == paymentId, ct);
            var outbox = await db.OutboxMessages.SingleAsync(x => x.Id == paymentId.Value, ct);
            Assert.Equal(PaymentStatus.Succeeded, payment.Status);
            Assert.Null(outbox.ProcessedAt);
            Assert.Equal(PaymentSuccessOutboxBoundary.MessageType, outbox.MessageType);
        }
    }

    [Fact]
    public async Task Rolled_Back_Success_Commits_Neither_Payment_Nor_Outbox()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await PaymentMigrator.MigrateAsync(migrate, ct);
        }

        PaymentId paymentId;
        await using (var db = _postgres.CreateDbContext())
        {
            var payment = CreateInitiated();
            paymentId = payment.Id;
            db.Payments.Add(payment);
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var payment = await db.Payments
                .Include(x => x.Attempts)
                .Include(x => x.ExecutionSnapshot)
                .SingleAsync(x => x.Id == paymentId, ct);
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            ApplySuccess(db, payment, Now.Plus(Duration.FromMinutes(2)));
            await db.SaveChangesAsync(ct);
            await tx.RollbackAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var payment = await db.Payments.Include(x => x.Attempts).SingleAsync(x => x.Id == paymentId, ct);
            Assert.Equal(PaymentStatus.Pending, payment.Status);
            Assert.Equal(0, await db.OutboxMessages.CountAsync(x => x.Id == paymentId.Value, ct));
        }
    }

    [Fact]
    public async Task Undispatched_Outbox_Remains_Pending_After_Reload()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await PaymentMigrator.MigrateAsync(migrate, ct);
        }

        PaymentId paymentId;
        await using (var db = _postgres.CreateDbContext())
        {
            var payment = CreateInitiated();
            paymentId = payment.Id;
            db.Payments.Add(payment);
            ApplySuccess(db, payment, Now.Plus(Duration.FromMinutes(2)));
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var outbox = await db.OutboxMessages.SingleAsync(x => x.Id == paymentId.Value, ct);
            Assert.Null(outbox.ProcessedAt);
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                new PaymentSuccessOutboxDispatcher(db, new ThrowingHandler(), new FixedClock(Now.Plus(Duration.FromMinutes(3))))
                    .DispatchPendingAsync(cancellationToken: ct));
            Assert.Null((await db.OutboxMessages.SingleAsync(x => x.Id == paymentId.Value, ct)).ProcessedAt);
        }
    }

    private static PaymentAggregate CreateInitiated()
    {
        var payment = PaymentAggregate.Create(
            new BookingReference(Guid.CreateVersion7()),
            Now);
        payment.BindExecutionSnapshot(Guid.CreateVersion7(), new MoneyValue(110m, "IRR"), Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordProviderInitiation(
            attempt.Id,
            Now.Plus(Duration.FromMinutes(1)),
            new ProviderKey("test"),
            new ProviderRequestReference("req-outbox-" + payment.Id.Value.ToString("N")),
            new ProviderTransactionReference("txn-outbox-" + payment.Id.Value.ToString("N")));
        return payment;
    }

    private static void ApplySuccess(PaymentDbContext db, PaymentAggregate payment, Instant now)
    {
        var status = VerifiedProviderOutcomeApplier.ApplyVerification(
            payment,
            payment.Attempts.Single(),
            new PaymentVerificationResult
            {
                Outcome = ProviderVerificationOutcome.Succeeded,
                ProviderKey = new ProviderKey("test"),
                ReportedAmount = 110m,
                ReportedCurrencyCode = "IRR",
            },
            now);
        PaymentSuccessOutboxWriter.EnqueueIfSucceeded(db, payment, now, status);
    }

    private sealed class ThrowingHandler : IPaymentSucceededIntegrationHandler
    {
        public Task HandleAsync(PaymentSucceededIntegrationEvent message, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("transient-consumer-failure");
    }

    private sealed class FixedClock : IClock
    {
        private readonly Instant _instant;

        public FixedClock(Instant instant) => _instant = instant;

        public Instant GetCurrentInstant() => _instant;
    }
}

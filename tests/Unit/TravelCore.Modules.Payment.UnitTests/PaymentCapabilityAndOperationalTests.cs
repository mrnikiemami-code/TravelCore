using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using TravelCore.Modules.Payment.Infrastructure;
using TravelCore.Modules.Payment.Infrastructure.Options;
using TravelCore.Modules.Payment.Infrastructure.Providers;
using TravelCore.Modules.Payment.Infrastructure.Services;
using TravelCore.Modules.Payment.UnitTests.Fakes;
using Xunit;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Payment.UnitTests;

public sealed class PaymentCapabilityAndOperationalTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 16, 0);
    private static readonly BookingReference Booking =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000880"));
    private static readonly ProviderKey TestKey = new("test");

    [Fact]
    public void Capability_Model_Is_Explicit_And_Not_Inferred_From_Name()
    {
        Assert.Equal(
            new[]
            {
                "RedirectInitiation",
                "CallbackVerification",
                "PaymentStatusQuery",
                "RefundInitiation",
                "RefundVerification",
                "RefundStatusQuery",
            },
            PaymentProviderCapabilitySet.ExactValues);
        Assert.Equal("NONE", PaymentProviderTrustBoundary.NamedProviderSelected);
        Assert.False(PaymentProviderTrustBoundary.NamedProductionAdapterImplemented);
        Assert.Equal("READY FOR ADAPTERS", PaymentProviderTrustBoundary.ProviderInfrastructurePosture);
        Assert.Equal("NOT CONFIGURED / NONE", PaymentProviderTrustBoundary.ProductionProviderPosture);
        Assert.False(PaymentOperationalBoundary.PublicOperationalEndpointImplemented);
        Assert.False(PaymentOperationalBoundary.ManualPaymentMutationImplemented);
        Assert.False(PaymentOperationalBoundary.ManualRefundMutationImplemented);
        Assert.Equal("AuthoritativeProviderQuery", PaymentOperationalBoundary.RecheckOutcomeSource);
        var fake = new FakePaymentProviderGateway(TestKey);
        Assert.True(fake.Capabilities.HasFlag(PaymentProviderCapability.RedirectInitiation));
        Assert.True(fake.Capabilities.HasFlag(PaymentProviderCapability.RefundInitiation));
        var resolver = new PaymentProviderResolver([fake]);
        var descriptor = resolver.Describe(TestKey);
        Assert.NotNull(descriptor);
        Assert.Equal(fake.Capabilities, descriptor.Capabilities);
        Assert.False(descriptor.AvailableForPublicInitiation);
    }

    [Fact]
    public void Duplicate_ProviderKey_Is_Rejected()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            new PaymentProviderResolver(
            [
                new FakePaymentProviderGateway(TestKey),
                new FakePaymentProviderGateway(TestKey),
            ]));
        Assert.Contains("Duplicate ProviderKey", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Disabled_And_Unknown_Providers_Fail_Safely()
    {
        var fake = new FakePaymentProviderGateway(TestKey);
        var resolver = new PaymentProviderResolver(
            [fake],
            Options.Create(new PaymentProviderOptions { DisabledProviderKeys = ["test"] }));
        Assert.Equal(ProviderCapabilityStatus.DisabledProvider, resolver.Check(TestKey, PaymentProviderCapability.None));
        Assert.Null(resolver.Resolve(TestKey));
        Assert.False(resolver.Describe(TestKey)!.Enabled);
        Assert.Equal(
            ProviderCapabilityStatus.UnknownProvider,
            resolver.Check(new ProviderKey("unknown"), PaymentProviderCapability.RedirectInitiation));
        Assert.Null(resolver.Resolve(new ProviderKey("unknown")));
    }

    [Fact]
    public void Unsupported_Status_Query_Does_Not_Mutate()
    {
        var fake = new FakePaymentProviderGateway(
            TestKey,
            PaymentProviderCapability.RedirectInitiation | PaymentProviderCapability.CallbackVerification);
        var resolver = new PaymentProviderResolver([fake]);
        Assert.Equal(
            ProviderCapabilityStatus.UnsupportedCapability,
            resolver.Check(TestKey, PaymentProviderCapability.PaymentStatusQuery));
        Assert.Equal(
            ProviderCapabilityStatus.UnsupportedCapability,
            resolver.Check(TestKey, PaymentProviderCapability.RefundInitiation));
    }

    [Fact]
    public async Task Operational_Read_Returns_Safe_Facts_Without_Mutation_Surface()
    {
        await using var db = CreateDb();
        var payment = PaymentAggregate.Create(Booking, Now);
        payment.BindExecutionSnapshot(Guid.CreateVersion7(), new MoneyValue(1000m, "USD"), Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordProviderInitiation(
            attempt.Id,
            Now.Plus(Duration.FromMinutes(1)),
            TestKey,
            new ProviderRequestReference("req-ops-1"),
            new ProviderTransactionReference("txn-ops-1"));
        payment.RecordAuthoritativeCollectionSuccess(attempt.Id, Now.Plus(Duration.FromMinutes(2)));
        db.Payments.Add(payment);
        var refund = Refund.CreateForSucceededPayment(payment, Now.Plus(Duration.FromMinutes(3)));
        db.Refunds.Add(refund);
        db.ReconciliationIssues.Add(
            PaymentReconciliationIssue.Create(
                payment.Id,
                attempt.Id,
                PaymentReconciliationIssueKind.AmountMismatch,
                Now));
        await db.SaveChangesAsync();

        var fake = new FakePaymentProviderGateway(TestKey);
        var resolver = new PaymentProviderResolver([fake]);
        var query = new PaymentOperationalQueryService(
            db,
            resolver,
            new PaymentAttemptRecheckService(db, resolver, new FixedClock(Now)),
            new RefundAttemptRecheckService(db, resolver, new FixedClock(Now)));
        var read = await query.GetByPaymentIdAsync(payment.Id.Value);
        Assert.NotNull(read);
        Assert.Equal(payment.Id.Value, read.PaymentId);
        Assert.Equal(Booking.BookingId, read.BookingId);
        Assert.Equal("Succeeded", read.PaymentStatus);
        Assert.Equal(1000m, read.Amount);
        Assert.Equal("USD", read.CurrencyCode);
        Assert.Single(read.Attempts);
        Assert.Equal("req-ops-1", read.Attempts[0].ProviderRequestReference);
        Assert.NotNull(read.Refund);
        Assert.Equal("Pending", read.Refund.Status);
        Assert.Contains("AmountMismatch", read.ReconciliationKinds);
        Assert.Equal("CompensationPending", read.CompensationState);
        Assert.Null(read.GetType().GetProperty("Email"));
        Assert.Null(read.GetType().GetProperty("AccessToken"));
        Assert.False(typeof(IPaymentOperationalQuery).GetMethods().Any(m =>
            m.Name.Contains("SetStatus", StringComparison.Ordinal)
            || m.Name.Contains("ForceSuccess", StringComparison.Ordinal)
            || m.Name.Contains("MarkPaid", StringComparison.Ordinal)
            || m.Name.Contains("MarkRefunded", StringComparison.Ordinal)
            || m.Name.Contains("ForceConfirm", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task Operational_Recheck_Uses_Provider_Query_And_Reports_Unsupported()
    {
        await using var db = CreateDb();
        var payment = PaymentAggregate.Create(Booking, Now);
        payment.BindExecutionSnapshot(Guid.CreateVersion7(), new MoneyValue(1000m, "USD"), Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordProviderInitiation(
            attempt.Id,
            Now.Plus(Duration.FromMinutes(1)),
            TestKey,
            new ProviderRequestReference("req-ops-2"),
            new ProviderTransactionReference("txn-ops-2"));
        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        var limited = new FakePaymentProviderGateway(TestKey, PaymentProviderCapability.RedirectInitiation);
        var limitedResolver = new PaymentProviderResolver([limited]);
        var limitedQuery = new PaymentOperationalQueryService(
            db,
            limitedResolver,
            new PaymentAttemptRecheckService(db, limitedResolver, new FixedClock(Now)),
            new RefundAttemptRecheckService(db, limitedResolver, new FixedClock(Now)));
        Assert.Equal(
            ProviderCapabilityStatus.UnsupportedCapability,
            await limitedQuery.RecheckPaymentAttemptAsync(attempt.Id.Value));
        var unchanged = await db.Payments.Include(x => x.Attempts).SingleAsync();
        Assert.Equal(PaymentAttemptStatus.Initiated, unchanged.Attempts.Single().Status);
        Assert.Equal(PaymentStatus.Pending, unchanged.Status);

        var full = new FakePaymentProviderGateway(TestKey)
        {
            NextVerification = ProviderVerificationOutcome.Succeeded,
            ReportedAmount = 1000m,
            ReportedCurrencyCode = "USD",
        };
        var fullResolver = new PaymentProviderResolver([full]);
        var fullQuery = new PaymentOperationalQueryService(
            db,
            fullResolver,
            new PaymentAttemptRecheckService(db, fullResolver, new FixedClock(Now.Plus(Duration.FromMinutes(4)))),
            new RefundAttemptRecheckService(db, fullResolver, new FixedClock(Now.Plus(Duration.FromMinutes(4)))));
        Assert.Equal(
            ProviderCapabilityStatus.Available,
            await fullQuery.RecheckPaymentAttemptAsync(attempt.Id.Value));
        var applied = await db.Payments.Include(x => x.Attempts).SingleAsync();
        Assert.Equal(PaymentStatus.Succeeded, applied.Status);
    }

    [Fact]
    public async Task Refund_Initiation_Requires_Refund_Capability()
    {
        await using var db = CreateDb();
        var payment = PaymentAggregate.Create(Booking, Now);
        payment.BindExecutionSnapshot(Guid.CreateVersion7(), new MoneyValue(110m, "IRR"), Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordProviderInitiation(
            attempt.Id,
            Now.Plus(Duration.FromMinutes(1)),
            TestKey,
            new ProviderRequestReference("req-cap-1"),
            new ProviderTransactionReference("txn-cap-1"));
        payment.RecordAuthoritativeCollectionSuccess(attempt.Id, Now.Plus(Duration.FromMinutes(2)));
        db.Payments.Add(payment);
        var refund = Refund.CreateForSucceededPayment(payment, Now.Plus(Duration.FromMinutes(3)));
        db.Refunds.Add(refund);
        await db.SaveChangesAsync();

        var fake = new FakePaymentProviderGateway(TestKey, PaymentProviderCapability.RedirectInitiation);
        var service = new RefundInitiationService(db, new PaymentProviderResolver([fake]), new FixedClock(Now.Plus(Duration.FromMinutes(4))));
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.InitiateAsync(refund.Id));
        Assert.Equal("Provider does not support RefundInitiation.", ex.Message);
        var reloaded = await db.Refunds.Include(x => x.Attempts).SingleAsync();
        Assert.Equal(RefundStatus.Pending, reloaded.Status);
        Assert.Empty(reloaded.Attempts);
        Assert.Equal(PaymentStatus.Succeeded, (await db.Payments.SingleAsync()).Status);
    }

    private static PaymentDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new PaymentDbContext(options);
    }

    private sealed class FixedClock : IClock
    {
        private readonly Instant _instant;
        public FixedClock(Instant instant) => _instant = instant;
        public Instant GetCurrentInstant() => _instant;
    }
}

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

namespace TravelCore.Modules.Payment.UnitTests;

public sealed class PaymentProviderBoundaryTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 10, 0);
    private static readonly BookingReference Booking =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000330"));
    private static readonly ProviderKey TestKey = new("test");

    [Fact]
    public void Trust_Invariants_Are_Locked()
    {
        Assert.Equal("BrowserReturn != PaymentSuccess", PaymentProviderTrustBoundary.BrowserReturnIsNotPaymentSuccess);
        Assert.Equal("UnverifiedCallback != PaymentSuccess", PaymentProviderTrustBoundary.UnverifiedCallbackIsNotPaymentSuccess);
        Assert.Equal("ClientSuccessFlag != PaymentSuccess", PaymentProviderTrustBoundary.ClientSuccessFlagIsNotPaymentSuccess);
        Assert.Equal("ProviderRedirect != PaymentSuccess", PaymentProviderTrustBoundary.ProviderRedirectIsNotPaymentSuccess);
        Assert.Equal("ProviderReference != PaymentId", PaymentProviderTrustBoundary.ProviderReferenceIsNotPaymentId);
        Assert.Equal("ProviderReference != PaymentAttemptId", PaymentProviderTrustBoundary.ProviderReferenceIsNotPaymentAttemptId);
        Assert.Equal("NetworkTimeout != PaymentAttemptFailed", PaymentProviderTrustBoundary.NetworkTimeoutIsNotAttemptFailed);
        Assert.Equal("NONE", PaymentProviderTrustBoundary.NamedProviderSelected);
        Assert.Equal("DeferredToR5", PaymentProviderTrustBoundary.AmountMismatchEnforcement);
        Assert.True(PaymentProviderTrustBoundary.ProviderPortImplemented);
        Assert.False(PaymentProviderTrustBoundary.NamedProductionAdapterImplemented);
        Assert.False(PaymentProviderTrustBoundary.ProductionFakeProviderRegistered);
        Assert.False(PaymentProviderTrustBoundary.AmountMismatchEnforcementImplemented);
        Assert.False(PaymentLifecycleBoundary.CallerControlledSuccessImplemented);
        Assert.False(PaymentLifecycleBoundary.PublicSuccessEndpointImplemented);
        Assert.False(PaymentLifecycleBoundary.BookingConfirmImplemented);
        Assert.False(PaymentLifecycleBoundary.RefundImplemented);
        Assert.Null(typeof(PaymentDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Payment.Domain.Refund"));
        Assert.Null(typeof(PaymentDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Payment.Domain.StripeStatus"));
    }

    [Fact]
    public void Provider_Initiation_Records_Neutral_Reference_And_Leaves_Payment_Pending()
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        var attempt = payment.CreateAttempt(Now);
        var result = new PaymentInitiationResult
        {
            Outcome = PaymentInitiationOutcome.Initiated,
            ProviderKey = TestKey,
            RequestReference = new ProviderRequestReference("req-1"),
            TransactionReference = new ProviderTransactionReference("txn-1"),
            RedirectUri = new Uri("https://example.test/pay"),
        };

        VerifiedProviderOutcomeApplier.ApplyInitiation(payment, attempt, result, Instant.FromUtc(2026, 8, 18, 10, 5));

        Assert.Equal(PaymentAttemptStatus.Initiated, attempt.Status);
        Assert.Equal(TestKey, attempt.ProviderKey);
        Assert.Equal("req-1", attempt.ProviderRequestReference!.Value.Value);
        Assert.Equal("txn-1", attempt.ProviderTransactionReference!.Value.Value);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.NotEqual(attempt.Id.Value, Guid.Parse("00000000-0000-0000-0000-000000000001"));
        Assert.NotEqual(payment.Id.Value.ToString("D"), attempt.ProviderRequestReference!.Value.Value);
    }

    [Fact]
    public void Definitive_Initiation_Failure_Fails_Attempt_And_Keeps_Payment_Pending()
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        var attempt = payment.CreateAttempt(Now);
        VerifiedProviderOutcomeApplier.ApplyInitiation(
            payment,
            attempt,
            new PaymentInitiationResult
            {
                Outcome = PaymentInitiationOutcome.DefinitiveFailure,
                ProviderKey = TestKey,
            },
            Instant.FromUtc(2026, 8, 18, 10, 6));

        Assert.Equal(PaymentAttemptStatus.Failed, attempt.Status);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
    }

    [Fact]
    public void Ambiguous_Initiation_Does_Not_Fabricate_Failed_Or_Succeeded()
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        var attempt = payment.CreateAttempt(Now);
        VerifiedProviderOutcomeApplier.ApplyInitiation(
            payment,
            attempt,
            new PaymentInitiationResult
            {
                Outcome = PaymentInitiationOutcome.Unknown,
                ProviderKey = TestKey,
            },
            Instant.FromUtc(2026, 8, 18, 10, 7));

        Assert.Equal(PaymentAttemptStatus.Created, attempt.Status);
        Assert.Equal(TestKey, attempt.ProviderKey);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Null(payment.SucceededAt);
    }

    [Fact]
    public void Verified_Provider_Success_Transitions_Attempt_And_Payment()
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordProviderInitiation(
            attempt.Id,
            Instant.FromUtc(2026, 8, 18, 10, 8),
            TestKey,
            new ProviderRequestReference("req-ok"),
            new ProviderTransactionReference("txn-ok"));

        VerifiedProviderOutcomeApplier.ApplyVerification(
            payment,
            attempt,
            ProviderVerificationOutcome.Succeeded,
            Instant.FromUtc(2026, 8, 18, 10, 9));

        Assert.Equal(PaymentAttemptStatus.Succeeded, attempt.Status);
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
        VerifiedProviderOutcomeApplier.ApplyVerification(
            payment,
            attempt,
            ProviderVerificationOutcome.Succeeded,
            Instant.FromUtc(2026, 8, 18, 10, 10));
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
        Assert.Equal(1, payment.Attempts.Count(item => item.Status == PaymentAttemptStatus.Succeeded));
    }

    [Fact]
    public void Verified_Failure_Fails_Attempt_And_Keeps_Payment_Pending()
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        var attempt = payment.CreateAttempt(Now);
        VerifiedProviderOutcomeApplier.ApplyVerification(
            payment,
            attempt,
            ProviderVerificationOutcome.Failed,
            Instant.FromUtc(2026, 8, 18, 10, 11));

        Assert.Equal(PaymentAttemptStatus.Failed, attempt.Status);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
    }

    [Fact]
    public void Verified_Pending_Leaves_Payment_Pending()
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        var attempt = payment.CreateAttempt(Now);
        VerifiedProviderOutcomeApplier.ApplyVerification(
            payment,
            attempt,
            ProviderVerificationOutcome.PendingUnknown,
            Instant.FromUtc(2026, 8, 18, 10, 12));

        Assert.Equal(PaymentAttemptStatus.Created, attempt.Status);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
    }

    [Fact]
    public void Failed_Attempt_Does_Not_Flip_To_Succeeded()
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordAttemptFailure(attempt.Id, Instant.FromUtc(2026, 8, 18, 10, 13));
        VerifiedProviderOutcomeApplier.ApplyVerification(
            payment,
            attempt,
            ProviderVerificationOutcome.Succeeded,
            Instant.FromUtc(2026, 8, 18, 10, 14));

        Assert.Equal(PaymentAttemptStatus.Failed, attempt.Status);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
    }

    [Fact]
    public async Task Unverified_Callback_Cannot_Mark_Success()
    {
        await using var db = CreateDb();
        var fake = new FakePaymentProviderGateway(TestKey);
        var payment = PaymentAggregate.Create(Booking, Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordProviderInitiation(
            attempt.Id,
            Instant.FromUtc(2026, 8, 18, 10, 15),
            TestKey,
            fake.RequestReference,
            fake.TransactionReference);
        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        var processor = new PaymentCallbackProcessor(
            db,
            new PaymentProviderResolver([fake]),
            new FixedClock(Instant.FromUtc(2026, 8, 18, 10, 16)));
        var result = await processor.ProcessAsync(new PaymentCallbackEnvelope
        {
            ProviderKey = TestKey,
            Query = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["success"] = "true",
            },
            Body = """{"success":true}""",
        });

        Assert.Equal(PaymentCallbackProcessStatus.Unverified, result.Status);
        await db.Entry(payment).ReloadAsync();
        var reloaded = await db.Payments.Include(item => item.Attempts).SingleAsync(item => item.Id == payment.Id);
        Assert.Equal(PaymentStatus.Pending, reloaded.Status);
        Assert.DoesNotContain(reloaded.Attempts, item => item.Status == PaymentAttemptStatus.Succeeded);
    }

    [Fact]
    public async Task Verified_Callback_Can_Mark_Success()
    {
        await using var db = CreateDb();
        var fake = new FakePaymentProviderGateway(TestKey)
        {
            NextVerification = ProviderVerificationOutcome.Succeeded,
        };
        var payment = PaymentAggregate.Create(Booking, Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordProviderInitiation(
            attempt.Id,
            Instant.FromUtc(2026, 8, 18, 10, 17),
            TestKey,
            fake.RequestReference,
            fake.TransactionReference);
        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        var processor = new PaymentCallbackProcessor(
            db,
            new PaymentProviderResolver([fake]),
            new FixedClock(Instant.FromUtc(2026, 8, 18, 10, 18)));
        var result = await processor.ProcessAsync(new PaymentCallbackEnvelope
        {
            ProviderKey = TestKey,
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [FakePaymentProviderGateway.VerifiedHeaderName] = "true",
            },
        });

        Assert.Equal(PaymentCallbackProcessStatus.Applied, result.Status);
        var reloaded = await db.Payments.Include(item => item.Attempts).SingleAsync(item => item.Id == payment.Id);
        Assert.Equal(PaymentStatus.Succeeded, reloaded.Status);
    }

    [Fact]
    public async Task Unknown_Provider_Callback_Cannot_Mutate_Payment()
    {
        await using var db = CreateDb();
        var payment = PaymentAggregate.Create(Booking, Now);
        payment.CreateAttempt(Now);
        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        var processor = new PaymentCallbackProcessor(
            db,
            new PaymentProviderResolver([]),
            new FixedClock(Now));
        var result = await processor.ProcessAsync(new PaymentCallbackEnvelope
        {
            ProviderKey = new ProviderKey("other"),
            Body = """{"success":true}""",
        });

        Assert.Equal(PaymentCallbackProcessStatus.UnknownProvider, result.Status);
        var reloaded = await db.Payments.Include(item => item.Attempts).SingleAsync(item => item.Id == payment.Id);
        Assert.Equal(PaymentStatus.Pending, reloaded.Status);
    }

    [Fact]
    public async Task Unknown_Callback_Correlation_Cannot_Mutate_Or_Create_Payment()
    {
        await using var db = CreateDb();
        var fake = new FakePaymentProviderGateway(TestKey);
        var processor = new PaymentCallbackProcessor(
            db,
            new PaymentProviderResolver([fake]),
            new FixedClock(Now));
        var result = await processor.ProcessAsync(new PaymentCallbackEnvelope
        {
            ProviderKey = TestKey,
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [FakePaymentProviderGateway.VerifiedHeaderName] = "true",
            },
        });

        Assert.Equal(PaymentCallbackProcessStatus.UnknownAttempt, result.Status);
        Assert.Equal(0, await db.Payments.CountAsync());
    }

    [Fact]
    public async Task Initiation_Service_Uses_Trusted_Provider_And_Network_Failure_Is_Unknown()
    {
        await using var db = CreateDb();
        var fake = new FakePaymentProviderGateway(TestKey) { ThrowOnInitiate = true };
        var payment = PaymentAggregate.Create(Booking, Now);
        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        var service = new PaymentInitiationService(
            db,
            new PaymentProviderResolver([fake]),
            Options.Create(new PaymentProviderOptions { DefaultProviderKey = "test" }),
            new FixedClock(Instant.FromUtc(2026, 8, 18, 10, 20)));
        var result = await service.InitiateAsync(payment.Id);

        Assert.Equal(PaymentInitiationOutcome.Unknown, result.Outcome);
        var reloaded = await db.Payments.Include(item => item.Attempts).SingleAsync(item => item.Id == payment.Id);
        Assert.Equal(PaymentStatus.Pending, reloaded.Status);
        Assert.Equal(PaymentAttemptStatus.Created, reloaded.Attempts.Single().Status);
        Assert.NotEqual(PaymentAttemptStatus.Failed, reloaded.Attempts.Single().Status);
        Assert.NotEqual(PaymentAttemptStatus.Succeeded, reloaded.Attempts.Single().Status);
    }

    [Fact]
    public async Task Initiation_Service_Records_Provider_Reference_On_Success()
    {
        await using var db = CreateDb();
        var fake = new FakePaymentProviderGateway(TestKey);
        var payment = PaymentAggregate.Create(Booking, Now);
        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        var service = new PaymentInitiationService(
            db,
            new PaymentProviderResolver([fake]),
            Options.Create(new PaymentProviderOptions { DefaultProviderKey = "test" }),
            new FixedClock(Instant.FromUtc(2026, 8, 18, 10, 21)));
        var result = await service.InitiateAsync(payment.Id);

        Assert.Equal(PaymentInitiationOutcome.Initiated, result.Outcome);
        var reloaded = await db.Payments.Include(item => item.Attempts).SingleAsync(item => item.Id == payment.Id);
        var attempt = reloaded.Attempts.Single();
        Assert.Equal(PaymentAttemptStatus.Initiated, attempt.Status);
        Assert.Equal(TestKey, attempt.ProviderKey);
        Assert.Equal(fake.RequestReference, attempt.ProviderRequestReference);
        Assert.Equal(PaymentStatus.Pending, reloaded.Status);
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

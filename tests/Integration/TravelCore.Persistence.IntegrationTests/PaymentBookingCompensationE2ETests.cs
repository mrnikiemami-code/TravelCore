using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Booking.Infrastructure;
using TravelCore.Modules.Booking.Infrastructure.Services;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using TravelCore.Modules.Payment.Infrastructure;
using TravelCore.Modules.Payment.Infrastructure.Providers;
using TravelCore.Modules.Payment.Infrastructure.Services;
using TravelCore.Modules.Pricing.Contracts;
using Xunit;
using BookingAggregate = TravelCore.Modules.Booking.Domain.Booking;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(PaymentBookingCompensationCollection))]
public sealed class PaymentBookingCompensationE2ETests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 23, 0);
    private static readonly ProviderKey TestKey = new("test");
    private readonly PaymentBookingCompensationContainerFixture _postgres;

    public PaymentBookingCompensationE2ETests(PaymentBookingCompensationContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Expired_Hold_Compensation_Refunds_And_Cancels_Pending()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedAsync(ct, holdMinutes: 1);
        await using (var db = _postgres.CreateBookingDbContext())
        {
            var holdId = (await db.CapacityHolds.SingleAsync(x => x.BookingId == ctx.BookingId, ct)).Id;
            await new BookingCapacityService(db).ExpireAsync(holdId, ctx.Now.Plus(Duration.FromMinutes(2)), ct);
        }

        await ConfirmAsync(ctx, ctx.Now.Plus(Duration.FromMinutes(2)), ct);
        await using (var pay = _postgres.CreatePaymentDbContext())
        {
            Assert.Equal(0, await pay.Refunds.CountAsync(x => x.PaymentId == ctx.PaymentId, ct));
        }

        await DispatchCompensationAndRefundAsync(ctx, ct);

        await using var bookingDb = _postgres.CreateBookingDbContext();
        var booking = await bookingDb.Bookings.SingleAsync(x => x.Id == ctx.BookingId, ct);
        var hold = await bookingDb.CapacityHolds.SingleAsync(x => x.BookingId == ctx.BookingId, ct);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.Equal(CapacityHoldStatus.Expired, hold.Status);
        await using var paymentDb = _postgres.CreatePaymentDbContext();
        Assert.Equal(RefundStatus.Succeeded, (await paymentDb.Refunds.SingleAsync(x => x.PaymentId == ctx.PaymentId, ct)).Status);
        Assert.Equal(PaymentStatus.Succeeded, (await paymentDb.Payments.SingleAsync(x => x.Id == ctx.PaymentId, ct)).Status);
    }

    [Fact]
    public async Task Cancelled_Booking_Compensation_Refunds_Idempotently()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedAsync(ct);
        await using (var db = _postgres.CreateBookingDbContext())
        {
            await new BookingCancellationService(db)
                .CancelPendingAsync(ctx.BookingId, ctx.Now.Plus(Duration.FromMinutes(1)), ct);
        }

        await ConfirmAsync(ctx, ctx.Now.Plus(Duration.FromMinutes(2)), ct);
        await DispatchCompensationAndRefundAsync(ctx, ct);
        await DispatchCompensationAndRefundAsync(ctx, ct);

        await using var bookingDb = _postgres.CreateBookingDbContext();
        Assert.Equal(BookingStatus.Cancelled, (await bookingDb.Bookings.SingleAsync(x => x.Id == ctx.BookingId, ct)).Status);
        await using var paymentDb = _postgres.CreatePaymentDbContext();
        Assert.Equal(1, await paymentDb.Refunds.CountAsync(x => x.PaymentId == ctx.PaymentId, ct));
        Assert.Equal(RefundStatus.Succeeded, (await paymentDb.Refunds.SingleAsync(x => x.PaymentId == ctx.PaymentId, ct)).Status);
    }

    [Fact]
    public async Task Released_Hold_Compensation_Cancels_Without_Another_Release()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedAsync(ct);
        await using (var db = _postgres.CreateBookingDbContext())
        {
            var hold = await db.CapacityHolds.SingleAsync(x => x.BookingId == ctx.BookingId, ct);
            await new BookingCapacityService(db).ReleaseAsync(hold.Id, ctx.Now.Plus(Duration.FromMinutes(1)), ct);
        }

        await ConfirmAsync(ctx, ctx.Now.Plus(Duration.FromMinutes(2)), ct);
        await DispatchCompensationAndRefundAsync(ctx, ct);

        await using var bookingDb = _postgres.CreateBookingDbContext();
        Assert.Equal(BookingStatus.Cancelled, (await bookingDb.Bookings.SingleAsync(x => x.Id == ctx.BookingId, ct)).Status);
        Assert.Equal(
            CapacityHoldStatus.Released,
            (await bookingDb.CapacityHolds.SingleAsync(x => x.BookingId == ctx.BookingId, ct)).Status);
    }

    [Fact]
    public async Task Crash_Window_Keeps_Compensation_Pending_Until_Payment_Consumes()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedAsync(ct, holdMinutes: 1);
        await ConfirmAsync(ctx, ctx.Now.Plus(Duration.FromMinutes(2)), ct);

        await using (var bookingDb = _postgres.CreateBookingDbContext())
        {
            Assert.Null((await bookingDb.OutboxMessages.SingleAsync(x => x.Id == ctx.PaymentId.Value, ct)).ProcessedAt);
        }

        await using (var paymentDb = _postgres.CreatePaymentDbContext())
        {
            Assert.Equal(0, await paymentDb.Refunds.CountAsync(x => x.PaymentId == ctx.PaymentId, ct));
        }

        await DispatchCompensationAndRefundAsync(ctx, ct);
        await using var pay = _postgres.CreatePaymentDbContext();
        Assert.Equal(RefundStatus.Succeeded, (await pay.Refunds.SingleAsync(x => x.PaymentId == ctx.PaymentId, ct)).Status);
    }

    [Fact]
    public async Task Crash_Window_Keeps_Booking_Pending_Until_Refund_Success_Is_Consumed()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedAsync(ct, holdMinutes: 1);
        await ConfirmAsync(ctx, ctx.Now.Plus(Duration.FromMinutes(2)), ct);
        await DispatchCompensationOnlyAsync(ctx, ct);
        await RecheckRefundAsync(ctx, ct);

        await using (var bookingDb = _postgres.CreateBookingDbContext())
        {
            Assert.Equal(BookingStatus.Pending, (await bookingDb.Bookings.SingleAsync(x => x.Id == ctx.BookingId, ct)).Status);
        }

        await DispatchRefundSuccessAsync(ctx, ct);
        await using var verify = _postgres.CreateBookingDbContext();
        Assert.Equal(BookingStatus.Cancelled, (await verify.Bookings.SingleAsync(x => x.Id == ctx.BookingId, ct)).Status);
    }

    private async Task<Seeded> SeedAsync(CancellationToken ct, int holdMinutes = 30)
    {
        await using (var booking = _postgres.CreateBookingDbContext())
        {
            await BookingMigrator.MigrateAsync(booking, ct);
        }

        await using (var payment = _postgres.CreatePaymentDbContext())
        {
            await PaymentMigrator.MigrateAsync(payment, ct);
        }

        var departure = new TourDepartureReference(Guid.CreateVersion7());
        BookingId bookingId;
        Guid snapshotId;
        await using (var db = _postgres.CreateBookingDbContext())
        {
            var booking = BookingAggregate.Create(departure, Now);
            booking.SetContact(BookingContactSnapshot.Create("Pay User", "pay@example.com"));
            booking.AddPassenger("Pay", "User", TravelerCategory.Adult, 1);
            booking.AcceptQuote(Facts(departure.LogicalId), Now);
            db.Bookings.Add(booking);
            await db.SaveChangesAsync(ct);
            bookingId = booking.Id;
            snapshotId = booking.MonetarySnapshot!.Id.Value;
        }

        await using (var db = _postgres.CreateBookingDbContext())
        {
            await new BookingCapacityService(db).HoldAsync(
                new HoldCapacityCommand(
                    bookingId,
                    1,
                    5,
                    Now.Plus(Duration.FromMinutes(holdMinutes)),
                    Now,
                    "hold-" + bookingId.Value),
                ct);
        }

        PaymentId paymentId;
        await using (var db = _postgres.CreatePaymentDbContext())
        {
            var payment = PaymentAggregate.Create(new BookingReference(bookingId.Value), Now);
            payment.BindExecutionSnapshot(snapshotId, new MoneyValue(110m, "USD"), Now);
            var attempt = payment.CreateAttempt(Now);
            payment.RecordProviderInitiation(
                attempt.Id,
                Now.Plus(Duration.FromMinutes(1)),
                TestKey,
                new ProviderRequestReference("req-" + payment.Id.Value.ToString("N")[..12]),
                new ProviderTransactionReference("txn-" + payment.Id.Value.ToString("N")[..12]));
            payment.RecordAuthoritativeCollectionSuccess(attempt.Id, Now.Plus(Duration.FromMinutes(1)));
            db.Payments.Add(payment);
            await db.SaveChangesAsync(ct);
            paymentId = payment.Id;
        }

        return new Seeded(bookingId, paymentId, Now);
    }

    private async Task ConfirmAsync(Seeded ctx, Instant now, CancellationToken ct)
    {
        await using var db = _postgres.CreateBookingDbContext();
        await new BookingPaymentConfirmationService(db, new FakeEvidence(ctx))
            .ConfirmIfEligibleAsync(ctx.BookingId, now, ct);
    }

    private async Task DispatchCompensationAndRefundAsync(Seeded ctx, CancellationToken ct)
    {
        await DispatchCompensationOnlyAsync(ctx, ct);
        await RecheckRefundAsync(ctx, ct);
        await DispatchRefundSuccessAsync(ctx, ct);
    }

    private async Task DispatchCompensationOnlyAsync(Seeded ctx, CancellationToken ct)
    {
        await using var bookingDb = _postgres.CreateBookingDbContext();
        await using var paymentDb = _postgres.CreatePaymentDbContext();
        var fake = new TestRefundGateway();
        var clock = new FixedClock(Now.Plus(Duration.FromMinutes(10)));
        var handler = new BookingPaymentCompensationRequiredHandler(
            paymentDb,
            new RefundGetOrCreateService(paymentDb, clock),
            new RefundInitiationService(paymentDb, new PaymentProviderResolver([fake]), clock),
            clock);
        await new BookingCompensationOutboxDispatcher(bookingDb, handler, clock)
            .DispatchPendingAsync(cancellationToken: ct);
    }

    private async Task RecheckRefundAsync(Seeded ctx, CancellationToken ct)
    {
        await using var paymentDb = _postgres.CreatePaymentDbContext();
        var refund = await paymentDb.Refunds.Include(x => x.Attempts).SingleAsync(x => x.PaymentId == ctx.PaymentId, ct);
        var attempt = refund.Attempts.Single();
        var fake = new TestRefundGateway();
        await new RefundAttemptRecheckService(
            paymentDb,
            new PaymentProviderResolver([fake]),
            new FixedClock(Now.Plus(Duration.FromMinutes(11))))
            .RecheckAsync(attempt.Id, ct);
    }

    private async Task DispatchRefundSuccessAsync(Seeded ctx, CancellationToken ct)
    {
        await using var bookingDb = _postgres.CreateBookingDbContext();
        await using var paymentDb = _postgres.CreatePaymentDbContext();
        var clock = new FixedClock(Now.Plus(Duration.FromMinutes(12)));
        var handler = new BookingRefundSucceededIntegrationHandler(
            bookingDb,
            new BookingCancellationService(bookingDb),
            clock);
        await new RefundSucceededOutboxDispatcher(paymentDb, handler, clock)
            .DispatchPendingAsync(cancellationToken: ct);
    }

    private static AuthoritativeQuoteFacts Facts(Guid departureId) =>
        AuthoritativeQuoteFacts.Create(
            PricingQuoteReference.From(Guid.CreateVersion7()),
            Guid.CreateVersion7(),
            BookingOwnershipBoundary.InitialTarget,
            departureId,
            Now.Minus(Duration.FromMinutes(10)),
            Now.Plus(Duration.FromHours(6)),
            [
                new AuthoritativeQuoteComponentFact(
                    BookingMonetaryComponentKind.Base,
                    new MoneyValue(110m, "USD"),
                    0,
                    "BASE",
                    "Base")
            ]);

    private sealed record Seeded(BookingId BookingId, PaymentId PaymentId, Instant Now);

    private sealed class FakeEvidence : IPaymentSuccessEvidenceQuery
    {
        private readonly Seeded _ctx;

        public FakeEvidence(Seeded ctx) => _ctx = ctx;

        public Task<PaymentSuccessEvidenceRead?> GetByBookingIdAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<PaymentSuccessEvidenceRead?>(
                new PaymentSuccessEvidenceRead(
                    _ctx.PaymentId.Value,
                    _ctx.BookingId.Value,
                    "Succeeded",
                    110m,
                    "USD",
                    true));
    }

    private sealed class FixedClock : IClock
    {
        private readonly Instant _instant;
        public FixedClock(Instant instant) => _instant = instant;
        public Instant GetCurrentInstant() => _instant;
    }

    private sealed class TestRefundGateway : IPaymentProviderGateway
    {
        public ProviderKey Key => TestKey;

        public Task<PaymentInitiationResult> InitiatePaymentAsync(
            PaymentInitiationRequest request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<PaymentVerificationResult> VerifyPaymentAsync(
            PaymentVerificationRequest request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<PaymentVerificationResult> QueryPaymentStatusAsync(
            PaymentVerificationRequest request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<PaymentCallbackVerification> VerifyCallbackAsync(
            PaymentCallbackEnvelope envelope,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<PaymentInitiationResult> InitiateRefundAsync(
            RefundInitiationRequest request,
            CancellationToken cancellationToken = default)
        {
            var requestRef = new ProviderRequestReference("rref-" + request.RefundAttemptId.ToString("N")[..12]);
            var txnRef = new ProviderTransactionReference("rtxn-" + request.RefundAttemptId.ToString("N")[..12]);
            return Task.FromResult(new PaymentInitiationResult
            {
                Outcome = PaymentInitiationOutcome.Initiated,
                ProviderKey = Key,
                RequestReference = requestRef,
                TransactionReference = txnRef,
            });
        }

        public Task<PaymentVerificationResult> VerifyRefundAsync(
            PaymentVerificationRequest request,
            CancellationToken cancellationToken = default) =>
            QueryRefundStatusAsync(request, cancellationToken);

        public Task<PaymentVerificationResult> QueryRefundStatusAsync(
            PaymentVerificationRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new PaymentVerificationResult
            {
                Outcome = ProviderVerificationOutcome.Succeeded,
                ProviderKey = Key,
                RequestReference = request.RequestReference,
                TransactionReference = request.TransactionReference,
                ReportedAmount = 110m,
                ReportedCurrencyCode = "USD",
            });
    }
}

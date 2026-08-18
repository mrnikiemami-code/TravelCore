using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using TravelCore.Modules.Payment.Infrastructure;
using TravelCore.Modules.Payment.Infrastructure.Providers;
using TravelCore.Modules.Payment.Infrastructure.Services;
using TravelCore.Modules.Payment.UnitTests.Fakes;
using Xunit;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Payment.UnitTests;

public sealed class PaymentHotelBookingTargetTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 21, 0);
    private static readonly BookingReference Tour =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000901"));
    private static readonly HotelBookingPaymentReference Hotel =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000902"));
    private static readonly Guid HotelSnapshotId = Guid.Parse("0198b3e0-0000-7000-8000-000000000903");
    private static readonly ProviderKey TestKey = new("test");

    [Fact]
    public void Tour_Target_Create_Remains_Unchanged()
    {
        var payment = PaymentAggregate.Create(Tour, Now);
        Assert.Equal(PaymentTargetKind.TourBooking, payment.TargetKind);
        Assert.Equal(Tour, payment.Booking);
        Assert.Null(payment.HotelBooking);
        Assert.Equal(Tour.BookingId, payment.TargetReferenceId);
        Assert.Equal(
            new[] { PaymentTargetKind.TourBooking, PaymentTargetKind.HotelBooking },
            Enum.GetValues<PaymentTargetKind>());
        Assert.DoesNotContain("Order", Enum.GetNames<PaymentTargetKind>());
        Assert.DoesNotContain("Flight", Enum.GetNames<PaymentTargetKind>());
        Assert.DoesNotContain("Visa", Enum.GetNames<PaymentTargetKind>());
        Assert.DoesNotContain("Subscription", Enum.GetNames<PaymentTargetKind>());
        Assert.DoesNotContain("Generic", Enum.GetNames<PaymentTargetKind>());
        Assert.False(PaymentOwnershipBoundary.GeneralizedTargetTypeImplemented);
        Assert.Null(typeof(PaymentAggregate).GetProperty("TargetType"));
    }

    [Fact]
    public void Hotel_Target_Create_Is_Exactly_One_Target()
    {
        var payment = PaymentAggregate.CreateForHotel(Hotel, Now);
        Assert.Equal(PaymentTargetKind.HotelBooking, payment.TargetKind);
        Assert.Equal(Hotel, payment.HotelBooking);
        Assert.Null(payment.Booking);
        Assert.Equal(Hotel.HotelBookingId, payment.TargetReferenceId);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
    }

    [Fact]
    public void Same_Guid_Tour_And_Hotel_Are_Distinct_Payments()
    {
        var id = Guid.Parse("0198b3e0-0000-7000-8000-000000000910");
        var tour = PaymentAggregate.Create(new BookingReference(id), Now);
        var hotel = PaymentAggregate.CreateForHotel(new HotelBookingPaymentReference(id), Now);
        Assert.NotEqual(tour.Id, hotel.Id);
        Assert.Equal(PaymentTargetKind.TourBooking, tour.TargetKind);
        Assert.Equal(PaymentTargetKind.HotelBooking, hotel.TargetKind);
        Assert.Equal(id, tour.TargetReferenceId);
        Assert.Equal(id, hotel.TargetReferenceId);
    }

    [Fact]
    public void Payment_Cannot_Have_Both_Targets()
    {
        Assert.Equal(1, typeof(PaymentAggregate).GetConstructors(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .Count(c => c.GetParameters().Length == 4));
        var payment = PaymentAggregate.CreateForHotel(Hotel, Now);
        Assert.True(payment.Booking is null ^ payment.HotelBooking is null);
    }

    [Fact]
    public async Task Duplicate_HotelBooking_GetOrCreate_Returns_Same_Payment()
    {
        await using var db = CreateDb();
        var clock = new FixedClock(Now);
        var service = new PaymentGetOrCreateService(db, clock);
        var first = await service.GetOrCreateAsync(Hotel);
        var second = await service.GetOrCreateAsync(Hotel);
        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, await db.Payments.CountAsync());
        Assert.Equal(PaymentTargetKind.HotelBooking, first.TargetKind);
    }

    [Fact]
    public async Task Hotel_Obligation_Snapshot_Is_Accepted_And_Changed_Obligation_Rejected()
    {
        await using var db = CreateDb();
        var payment = PaymentAggregate.CreateForHotel(Hotel, Now);
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        var obligations = new FakeHotelObligationQuery
        {
            Next = new HotelBookingPaymentObligationRead(
                Hotel.HotelBookingId,
                "Pending",
                1_000_000m,
                "IRR",
                HotelSnapshotId,
                PaymentEligible: true),
        };
        var service = new PaymentPreparationService(
            db,
            new MissingTourObligationQuery(),
            new FixedClock(Now),
            obligations);
        await service.PrepareAsync(payment.Id);
        await service.PrepareAsync(payment.Id);
        var first = await db.Payments.Include(x => x.ExecutionSnapshot).SingleAsync();
        Assert.Equal(1_000_000m, first.ExecutionSnapshot!.Amount.Amount);
        Assert.Equal("IRR", first.ExecutionSnapshot.Amount.Currency.Value);
        Assert.Equal(HotelSnapshotId, first.ExecutionSnapshot.BookingSnapshotId);

        obligations.Next = obligations.Next with { Amount = 999m };
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.PrepareAsync(payment.Id));
        var reloaded = await db.Payments.Include(x => x.ExecutionSnapshot).SingleAsync();
        Assert.Equal(1_000_000m, reloaded.ExecutionSnapshot!.Amount.Amount);
    }

    [Fact]
    public async Task Hotel_Payment_Success_Writes_Hotel_Outbox_Once()
    {
        await using var db = CreateDb();
        var fake = MatchingFake();
        var payment = await SeedHotelInitiatedAsync(db, fake);
        var processor = new PaymentCallbackProcessor(
            db,
            new PaymentProviderResolver([fake]),
            new FixedClock(Now.Plus(Duration.FromMinutes(2))));
        await processor.ProcessAsync(VerifiedEnvelope());
        await processor.ProcessAsync(VerifiedEnvelope());

        var row = Assert.Single(db.OutboxMessages);
        Assert.Equal(HotelBookingPaymentSuccessOutboxBoundary.MessageType, row.MessageType);
        Assert.NotEqual(PaymentSuccessOutboxBoundary.MessageType, row.MessageType);
        var evt = HotelBookingPaymentSucceededOutboxSerializer.Deserialize(row.Payload);
        Assert.Equal(payment.Id.Value, evt.PaymentId);
        Assert.Equal(Hotel.HotelBookingId, evt.HotelBookingId);
        Assert.Equal(1_000_000m, evt.Amount);
        Assert.Equal("IRR", evt.CurrencyCode);
        Assert.False(HotelBookingPaymentSuccessOutboxBoundary.EventMeansHotelBookingConfirmed);
    }

    [Fact]
    public async Task Hotel_Compensation_Creates_One_Refund_And_Keeps_Payment_Succeeded()
    {
        await using var db = CreateDb();
        var payment = await SeedHotelSucceededAsync(db);
        var handler = new HotelBookingPaymentCompensationRequiredHandler(
            db,
            new RefundGetOrCreateService(db, new FixedClock(Now.Plus(Duration.FromMinutes(3)))),
            new RefundInitiationService(db, new PaymentProviderResolver([MatchingRefundFake()]), new FixedClock(Now.Plus(Duration.FromMinutes(3)))),
            new FixedClock(Now.Plus(Duration.FromMinutes(3))));

        var message = new HotelBookingPaymentCompensationRequiredIntegrationEvent(
            Hotel.HotelBookingId,
            payment.Id.Value,
            "HoldExpired",
            Now.Plus(Duration.FromMinutes(3)));
        await handler.HandleAsync(message);
        await handler.HandleAsync(message);

        Assert.Equal(1, await db.Refunds.CountAsync());
        var refund = await db.Refunds.Include(x => x.Amount).SingleAsync();
        Assert.Equal(1_000_000m, refund.Amount.Amount);
        Assert.Equal("IRR", refund.Amount.Currency.Value);
        Assert.Equal(Hotel, refund.HotelBooking);
        Assert.Null(refund.Booking);
        Assert.Equal(PaymentStatus.Succeeded, (await db.Payments.SingleAsync()).Status);
        Assert.Equal(1, await db.CompensationInbox.CountAsync());
    }

    private static async Task<PaymentAggregate> SeedHotelInitiatedAsync(
        PaymentDbContext db,
        FakePaymentProviderGateway fake)
    {
        var payment = PaymentAggregate.CreateForHotel(Hotel, Now);
        payment.BindExecutionSnapshot(HotelSnapshotId, new MoneyValue(1_000_000m, "IRR"), Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordProviderInitiation(
            attempt.Id,
            Now.Plus(Duration.FromMinutes(1)),
            TestKey,
            fake.RequestReference,
            fake.TransactionReference);
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        fake.NextVerification = ProviderVerificationOutcome.Succeeded;
        fake.ReportedAmount = 1_000_000m;
        fake.ReportedCurrencyCode = "IRR";
        return payment;
    }

    private static async Task<PaymentAggregate> SeedHotelSucceededAsync(PaymentDbContext db)
    {
        var payment = PaymentAggregate.CreateForHotel(Hotel, Now);
        payment.BindExecutionSnapshot(HotelSnapshotId, new MoneyValue(1_000_000m, "IRR"), Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordProviderInitiation(
            attempt.Id,
            Now.Plus(Duration.FromMinutes(1)),
            TestKey,
            new ProviderRequestReference("req-hotel-1"),
            new ProviderTransactionReference("txn-hotel-1"));
        payment.RecordAuthoritativeCollectionSuccess(attempt.Id, Now.Plus(Duration.FromMinutes(2)));
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        return payment;
    }

    private static FakePaymentProviderGateway MatchingFake() =>
        new(TestKey)
        {
            NextVerification = ProviderVerificationOutcome.Succeeded,
            ReportedAmount = 1_000_000m,
            ReportedCurrencyCode = "IRR",
        };

    private static FakePaymentProviderGateway MatchingRefundFake() =>
        new(TestKey)
        {
            NextRefundInitiation = PaymentInitiationOutcome.Initiated,
            NextRefundVerification = ProviderVerificationOutcome.Succeeded,
            ReportedRefundAmount = 1_000_000m,
            ReportedRefundCurrencyCode = "IRR",
        };

    private static PaymentCallbackEnvelope VerifiedEnvelope() =>
        new()
        {
            ProviderKey = TestKey,
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [FakePaymentProviderGateway.VerifiedHeaderName] = "true",
            },
        };

    private static PaymentDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new PaymentDbContext(options);
    }

    private sealed class FakeHotelObligationQuery : IHotelBookingPaymentObligationQuery
    {
        public HotelBookingPaymentObligationRead? Next { get; set; }

        public Task<HotelBookingPaymentObligationRead?> GetByHotelBookingIdAsync(
            Guid hotelBookingId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Next);
    }

    private sealed class MissingTourObligationQuery : TravelCore.Modules.Booking.Contracts.IBookingPaymentObligationQuery
    {
        public Task<TravelCore.Modules.Booking.Contracts.BookingPaymentObligationRead?> GetByBookingIdAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<TravelCore.Modules.Booking.Contracts.BookingPaymentObligationRead?>(null);
    }

    private sealed class FixedClock(Instant now) : IClock
    {
        public Instant GetCurrentInstant() => now;
    }
}

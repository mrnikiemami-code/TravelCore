using Microsoft.EntityFrameworkCore;
using NodaTime;
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

public sealed class PaymentHotelBookingCancellationRefundTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 21, 0);
    private static readonly BookingReference Tour =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000911"));
    private static readonly HotelBookingPaymentReference Hotel =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000912"));
    private static readonly Guid HotelSnapshotId = Guid.Parse("0198b3e0-0000-7000-8000-000000000913");
    private static readonly ProviderKey TestKey = new("test");

    [Fact]
    public async Task Duplicate_Cancellation_Refund_Event_Creates_One_Full_Refund_From_Snapshot()
    {
        await using var db = CreateDb();
        var payment = await SeedHotelSucceededAsync(db);
        var handler = CreateHandler(db);
        var cancellationId = Guid.CreateVersion7();
        var message = new HotelBookingCancellationRefundRequiredIntegrationEvent(
            cancellationId,
            Hotel.HotelBookingId,
            payment.Id.Value,
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
        Assert.Equal(1, await db.HotelBookingCancellationRefundInbox.CountAsync());
        Assert.Equal(
            cancellationId,
            (await db.HotelBookingCancellationRefundInbox.SingleAsync()).HotelBookingCancellationId);
    }

    [Fact]
    public async Task Hotel_Cancellation_Refund_Cannot_Target_Tour_Payment()
    {
        await using var db = CreateDb();
        var tour = PaymentAggregate.Create(Tour, Now);
        tour.BindExecutionSnapshot(Guid.CreateVersion7(), new MoneyValue(500_000m, "IRR"), Now);
        var attempt = tour.CreateAttempt(Now);
        tour.RecordProviderInitiation(
            attempt.Id,
            Now.Plus(Duration.FromMinutes(1)),
            TestKey,
            new ProviderRequestReference("req-tour-1"),
            new ProviderTransactionReference("txn-tour-1"));
        tour.RecordAuthoritativeCollectionSuccess(attempt.Id, Now.Plus(Duration.FromMinutes(2)));
        db.Payments.Add(tour);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(new HotelBookingCancellationRefundRequiredIntegrationEvent(
                Guid.CreateVersion7(),
                Hotel.HotelBookingId,
                tour.Id.Value,
                Now.Plus(Duration.FromMinutes(3)))));
        Assert.Contains("HotelBooking", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, await db.Refunds.CountAsync());
        Assert.Equal(PaymentStatus.Succeeded, (await db.Payments.SingleAsync()).Status);
    }

    private static HotelBookingCancellationRefundRequiredHandler CreateHandler(PaymentDbContext db) =>
        new(
            db,
            new RefundGetOrCreateService(db, new FixedClock(Now.Plus(Duration.FromMinutes(3)))),
            new RefundInitiationService(
                db,
                new PaymentProviderResolver([MatchingRefundFake()]),
                new FixedClock(Now.Plus(Duration.FromMinutes(3)))),
            new FixedClock(Now.Plus(Duration.FromMinutes(3))));

    private static async Task<PaymentAggregate> SeedHotelSucceededAsync(PaymentDbContext db)
    {
        var payment = PaymentAggregate.CreateForHotel(Hotel, Now);
        payment.BindExecutionSnapshot(HotelSnapshotId, new MoneyValue(1_000_000m, "IRR"), Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordProviderInitiation(
            attempt.Id,
            Now.Plus(Duration.FromMinutes(1)),
            TestKey,
            new ProviderRequestReference("req-hotel-cancel-1"),
            new ProviderTransactionReference("txn-hotel-cancel-1"));
        payment.RecordAuthoritativeCollectionSuccess(attempt.Id, Now.Plus(Duration.FromMinutes(2)));
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        return payment;
    }

    private static FakePaymentProviderGateway MatchingRefundFake() =>
        new(TestKey)
        {
            NextRefundInitiation = PaymentInitiationOutcome.Initiated,
            NextRefundVerification = ProviderVerificationOutcome.Succeeded,
            ReportedRefundAmount = 1_000_000m,
            ReportedRefundCurrencyCode = "IRR",
        };

    private static PaymentDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new PaymentDbContext(options);
    }

    private sealed class FixedClock(Instant now) : IClock
    {
        public Instant GetCurrentInstant() => now;
    }
}

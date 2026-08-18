using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Money;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.HotelBooking.Infrastructure;
using TravelCore.Modules.HotelBooking.Infrastructure.Reservations;
using TravelCore.Modules.HotelBooking.Infrastructure.Services;
using Xunit;
using MoneyValue = TravelCore.Money.Money;
using Stay = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Modules.HotelBooking.UnitTests;

public sealed class HotelBookingPayNowOrchestrationTests
{
    private static readonly Instant T0 = Instant.FromUtc(2026, 8, 18, 12, 0);

    [Fact]
    public async Task Active_Hold_Is_Eligible_Requested_Hold_Is_Not()
    {
        await using var activeDb = CreateDb();
        var active = await SeedBookingAsync(activeDb, activateHold: true);
        var eligible = await new HotelBookingPaymentObligationQueryService(
            activeDb, new FixedClock(T0.Plus(Duration.FromMinutes(5))))
            .GetByHotelBookingIdAsync(active.Id.Value);
        Assert.NotNull(eligible);
        Assert.True(eligible.PaymentEligible);
        Assert.Equal(1_000_000m, eligible.Amount);
        Assert.Equal("IRR", eligible.CurrencyCode);

        await using var requestedDb = CreateDb();
        var requested = await SeedBookingAsync(requestedDb, activateHold: false);
        var requestedRead = await new HotelBookingPaymentObligationQueryService(
            requestedDb, new FixedClock(T0.Plus(Duration.FromMinutes(5))))
            .GetByHotelBookingIdAsync(requested.Id.Value);
        Assert.False(requestedRead!.PaymentEligible);
    }

    [Fact]
    public async Task Released_Expired_Cancelled_Confirmed_Are_Ineligible()
    {
        await using var releasedDb = CreateDb();
        var released = await SeedBookingAsync(releasedDb, activateHold: true);
        var hold = await releasedDb.HotelAvailabilityHolds.SingleAsync();
        hold.Release(T0.Plus(Duration.FromMinutes(10)));
        await releasedDb.SaveChangesAsync();
        var releasedRead = await new HotelBookingPaymentObligationQueryService(
            releasedDb, new FixedClock(T0.Plus(Duration.FromMinutes(11))))
            .GetByHotelBookingIdAsync(released.Id.Value);
        Assert.False(releasedRead!.PaymentEligible);

        await using var expiredDb = CreateDb();
        var expired = await SeedBookingAsync(expiredDb, activateHold: true);
        var expiredHold = await expiredDb.HotelAvailabilityHolds.SingleAsync();
        expiredHold.Expire(T0.Plus(Duration.FromHours(3)));
        await expiredDb.SaveChangesAsync();
        var expiredRead = await new HotelBookingPaymentObligationQueryService(
            expiredDb, new FixedClock(T0.Plus(Duration.FromHours(4))))
            .GetByHotelBookingIdAsync(expired.Id.Value);
        Assert.False(expiredRead!.PaymentEligible);

        await using var cancelledDb = CreateDb();
        var cancelled = await SeedBookingAsync(cancelledDb, activateHold: true);
        var cancelledBooking = await cancelledDb.HotelBookings.SingleAsync();
        cancelledBooking.CancelFromAuthoritativePaymentCompensation(T0.Plus(Duration.FromMinutes(2)));
        await cancelledDb.SaveChangesAsync();
        var cancelledRead = await new HotelBookingPaymentObligationQueryService(
            cancelledDb, new FixedClock(T0.Plus(Duration.FromMinutes(3))))
            .GetByHotelBookingIdAsync(cancelled.Id.Value);
        Assert.False(cancelledRead!.PaymentEligible);

        await using var confirmedDb = CreateDb();
        var confirmed = await SeedBookingAsync(confirmedDb, activateHold: true);
        var snapshot = await confirmedDb.HotelRateOfferSnapshots.Include(x => x.Monetary).SingleAsync();
        var reservation = HotelSupplierReservation.StartPending(confirmed.Id, "test-source", T0);
        var attempt = reservation.StartAttempt(T0);
        reservation.ConfirmAttempt(
            attempt.Id,
            T0.Plus(Duration.FromMinutes(1)),
            "src-res-1",
            "CNF-1",
            confirmed.Rooms.Select(r => r.Id).ToArray(),
            confirmed.Rooms.Select(r => r.Id).ToArray());
        var evidence = HotelBookingPaymentEvidence.Record(
            confirmed.Id, Guid.CreateVersion7(), 1_000_000m, "IRR", T0.Plus(Duration.FromMinutes(1)));
        confirmed.ConfirmFromAuthoritativePaymentAndSupplierEvidence(
            reservation, evidence, T0.Plus(Duration.FromMinutes(2)),
            confirmed.Place, confirmed.CheckInDate, confirmed.CheckOutDate,
            confirmed.Rooms.Select(r => r.Id).ToArray(), snapshot.Monetary.Total, true,
            snapshot.Monetary, []);
        await confirmedDb.SaveChangesAsync();
        var confirmedRead = await new HotelBookingPaymentObligationQueryService(
            confirmedDb, new FixedClock(T0.Plus(Duration.FromMinutes(3))))
            .GetByHotelBookingIdAsync(confirmed.Id.Value);
        Assert.False(confirmedRead!.PaymentEligible);
    }

    [Fact]
    public async Task Pay_First_Blocks_New_Supplier_Initiation_Until_Payment_Evidence()
    {
        await using var db = CreateDb();
        var booking = await SeedBookingAsync(db, activateHold: true);
        var source = new FakeHotelReservationSource
        {
            NextCreate = Complete(booking, booking.Rooms.Select(r => r.Id.Value).ToArray()),
        };
        var service = new HotelSupplierReservationService(
            db, new HotelReservationSourceResolver([source]), new FixedClock(T0.Plus(Duration.FromMinutes(5))));

        var blocked = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.InitiateAsync(booking.Id, "key-1"));
        Assert.Contains("Payment", blocked.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, source.CreateCalls);

        db.HotelBookingPaymentEvidence.Add(
            HotelBookingPaymentEvidence.Record(
                booking.Id, Guid.CreateVersion7(), 1_000_000m, "IRR", T0.Plus(Duration.FromMinutes(4))));
        await db.SaveChangesAsync();

        var reservation = await service.InitiateAsync(booking.Id, "key-1");
        Assert.Equal(1, source.CreateCalls);
        Assert.Equal(HotelSupplierReservationStatus.Confirmed, reservation.Status);
        var loaded = await db.HotelBookings.SingleAsync(x => x.Id == booking.Id);
        Assert.Equal(HotelBookingStatus.Confirmed, loaded.Status);
    }

    [Fact]
    public async Task Timeout_After_Payment_Does_Not_Create_Refund_Compensation()
    {
        await using var db = CreateDb();
        var booking = await SeedBookingAsync(db, activateHold: true);
        db.HotelBookingPaymentEvidence.Add(
            HotelBookingPaymentEvidence.Record(
                booking.Id, Guid.CreateVersion7(), 1_000_000m, "IRR", T0.Plus(Duration.FromMinutes(4))));
        await db.SaveChangesAsync();
        var source = new FakeHotelReservationSource { CreateException = new TimeoutException("network") };
        var service = new HotelSupplierReservationService(
            db, new HotelReservationSourceResolver([source]), new FixedClock(T0.Plus(Duration.FromMinutes(5))));
        var reservation = await service.InitiateAsync(booking.Id, "key-timeout");
        Assert.Equal(HotelSupplierReservationAttemptStatus.Initiated, reservation.Attempts.Single().Status);
        Assert.Equal(0, await db.PaymentCompensationEvidence.CountAsync());
        Assert.Equal(HotelBookingStatus.Pending, (await db.HotelBookings.SingleAsync()).Status);
    }

    [Fact]
    public async Task Hold_Expired_After_Payment_Requires_Compensation()
    {
        await using var db = CreateDb();
        var booking = await SeedBookingAsync(db, activateHold: true);
        var paymentId = Guid.CreateVersion7();
        db.HotelBookingPaymentEvidence.Add(
            HotelBookingPaymentEvidence.Record(booking.Id, paymentId, 1_000_000m, "IRR", T0.Plus(Duration.FromMinutes(4))));
        var hold = await db.HotelAvailabilityHolds.SingleAsync();
        hold.Expire(T0.Plus(Duration.FromHours(3)));
        await db.SaveChangesAsync();
        var source = new FakeHotelReservationSource
        {
            NextCreate = Complete(booking, booking.Rooms.Select(r => r.Id.Value).ToArray()),
        };
        var service = new HotelSupplierReservationService(
            db, new HotelReservationSourceResolver([source]), new FixedClock(T0.Plus(Duration.FromHours(4))));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.InitiateAsync(booking.Id, "key-expired"));
        var compensation = Assert.Single(await db.PaymentCompensationEvidence.ToListAsync());
        Assert.Equal(HotelBookingPaymentCompensationReason.HoldExpired, compensation.Reason);
        Assert.Equal(paymentId, compensation.PaymentId);
        Assert.Equal(1, await db.OutboxMessages.CountAsync(x =>
            x.MessageType == TravelCore.Modules.Payment.Contracts.HotelBookingCompensationOutboxBoundary.MessageType));
        Assert.Equal(0, source.CreateCalls);
    }

    [Fact]
    public async Task Supplier_Definitive_Failure_After_Payment_Requires_Compensation()
    {
        await using var db = CreateDb();
        var booking = await SeedBookingAsync(db, activateHold: true);
        var paymentId = Guid.CreateVersion7();
        db.HotelBookingPaymentEvidence.Add(
            HotelBookingPaymentEvidence.Record(booking.Id, paymentId, 1_000_000m, "IRR", T0.Plus(Duration.FromMinutes(4))));
        await db.SaveChangesAsync();
        var source = new FakeHotelReservationSource
        {
            NextCreate = new HotelReservationSourceResult(
                HotelReservationSourceOutcome.Failed, null, null, [], null, null),
        };
        var service = new HotelSupplierReservationService(
            db, new HotelReservationSourceResolver([source]), new FixedClock(T0.Plus(Duration.FromMinutes(5))));
        await service.InitiateAsync(booking.Id, "key-fail");
        var compensation = Assert.Single(await db.PaymentCompensationEvidence.ToListAsync());
        Assert.Equal(HotelBookingPaymentCompensationReason.SupplierReservationNotCreated, compensation.Reason);
        Assert.Equal(HotelBookingStatus.Pending, (await db.HotelBookings.SingleAsync()).Status);
    }

    private static HotelBookingDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<HotelBookingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new HotelBookingDbContext(options);
    }

    private static async Task<Stay> SeedBookingAsync(HotelBookingDbContext db, bool activateHold)
    {
        var seed = CreateSeed(activateHold);
        db.HotelBookings.Add(seed.Booking);
        db.HotelRateOfferSnapshots.Add(seed.Snapshot);
        db.HotelAvailabilityHolds.Add(seed.Hold);
        await db.SaveChangesAsync();
        return seed.Booking;
    }

    private static (Stay Booking, HotelRateOfferSnapshot Snapshot, HotelAvailabilityHold Hold) CreateSeed(
        bool activateHold)
    {
        var booking = Stay.Create(
            new HotelPlaceReference(Guid.Parse("0198b3e0-0000-7000-8000-000000000021")),
            new LocalDate(2026, 8, 18),
            new LocalDate(2026, 8, 20),
            HotelBookingContactSnapshot.Create(email: "lead@example.com"),
            [
                new RoomReservationSpecification(
                [
                    new HotelBookingGuestSpecification("Ada", "Lovelace", HotelGuestCategory.Adult, true),
                ]),
                new RoomReservationSpecification(
                [
                    new HotelBookingGuestSpecification("Alan", "Turing", HotelGuestCategory.Adult, false),
                ]),
            ]);
        var irr = CurrencyCode.Parse("IRR");
        var rooms = booking.Rooms.OrderBy(r => r.Ordinal).ToArray();
        var snapshot = HotelRateOfferSnapshot.Accept(
            booking,
            T0,
            booking.Place,
            booking.CheckInDate,
            booking.CheckOutDate,
            "test-source",
            $"offer-{booking.Id.Value:N}",
            T0.Minus(Duration.FromMinutes(1)),
            T0.Plus(Duration.FromHours(2)),
            new MoneyValue(1_000_000m, irr),
            [
                new HotelRoomRateLine(rooms[0].Id, new MoneyValue(400_000m, irr), "sel-1", "rate-1", "BB"),
                new HotelRoomRateLine(rooms[1].Id, new MoneyValue(600_000m, irr), "sel-2", "rate-2", "BB"),
            ],
            [
                new HotelCancellationPenaltyRuleDraft(T0, T0.Plus(Duration.FromDays(1)), new MoneyValue(0m, irr)),
                new HotelCancellationPenaltyRuleDraft(T0.Plus(Duration.FromDays(1)), null, new MoneyValue(1_000_000m, irr)),
            ]);
        var hold = HotelAvailabilityHold.StartRequested(
            booking.Id, "test-source", T0, booking.Rooms.Select(r => r.Id).ToArray());
        if (activateHold)
        {
            hold.Activate(
                T0.Plus(Duration.FromMinutes(1)),
                T0.Plus(Duration.FromHours(2)),
                $"src-hold-{booking.Id.Value:N}",
                booking.Rooms.ToDictionary(r => r.Id, r => $"sel-{r.Ordinal}"));
        }

        return (booking, snapshot, hold);
    }

    private static HotelReservationSourceResult Complete(Stay booking, Guid[] roomIds) =>
        new(
            HotelReservationSourceOutcome.Complete,
            $"src-res-{booking.Id.Value:N}",
            "CNF-1",
            roomIds,
            new MoneyValue(1_000_000m, CurrencyCode.Parse("IRR")),
            true);

    private sealed class FakeHotelReservationSource : IHotelReservationSource
    {
        public ReservationSourceKey Key { get; } = new("test-source");
        public bool RequiresActiveHold { get; set; } = true;
        public bool NotFoundProvesNoReservation { get; set; }
        public HotelReservationSourceResult? NextCreate { get; set; }
        public Exception? CreateException { get; set; }
        public int CreateCalls { get; private set; }

        public Task<HotelReservationSourceResult> CreateReservationAsync(
            HotelReservationRequest request,
            CancellationToken cancellationToken = default)
        {
            CreateCalls++;
            if (CreateException is not null)
            {
                throw CreateException;
            }

            return Task.FromResult(NextCreate ?? throw new InvalidOperationException("NextCreate is required."));
        }

        public Task<HotelReservationQueryResult> QueryReservationStatusAsync(
            string sourceReservationReference,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new HotelReservationQueryResult(
                HotelReservationQueryStatus.PendingUnknown,
                sourceReservationReference,
                [],
                null));

        public Task<HotelReservationCancellationSourceResult> InitiateCancellationAsync(
            HotelReservationCancellationRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new HotelReservationCancellationSourceResult(
                HotelReservationCancellationSourceOutcome.Unknown));

        public Task<HotelReservationCancellationQueryResult> QueryCancellationStatusAsync(
            HotelReservationCancellationQueryRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new HotelReservationCancellationQueryResult(
                HotelReservationCancellationQueryStatus.PendingUnknown));
    }

    private sealed class FixedClock(Instant now) : IClock
    {
        public Instant GetCurrentInstant() => now;
    }
}

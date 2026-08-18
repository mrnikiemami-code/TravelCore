using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Booking.Infrastructure;
using TravelCore.Modules.Booking.Infrastructure.Services;
using Xunit;
using BookingAggregate = TravelCore.Modules.Booking.Domain.Booking;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(BookingMigrationLifecycleCollection))]
public sealed class BookingCapacityPersistenceTests
{
    private readonly BookingMigrationLifecycleContainerFixture _postgres;

    public BookingCapacityPersistenceTests(BookingMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Concurrent_Holds_Capacity_1_Allow_Only_One_Success()
    {
        var ct = TestContext.Current.CancellationToken;
        await EnsureMigratedAsync(ct);
        var departure = NewDeparture();
        var now = Instant.FromUtc(2026, 8, 18, 8, 0);
        var expires = now.Plus(Duration.FromMinutes(5));
        var bookingA = await PersistBookingAsync(departure, now, ct);
        var bookingB = await PersistBookingAsync(departure, now, ct);

        var taskA = HoldOnNewContextAsync(bookingA, 1, 1, now, expires, "idem-a-" + bookingA.Value, ct);
        var taskB = HoldOnNewContextAsync(bookingB, 1, 1, now, expires, "idem-b-" + bookingB.Value, ct);
        var results = await Task.WhenAll(Capture(taskA), Capture(taskB));

        Assert.Equal(1, results.Count(x => x.Hold is not null));
        Assert.Equal(1, results.Count(x => x.Error is InsufficientCapacityException));
        Assert.Equal(1, await EffectiveSeatsAsync(departure, ct));
    }

    [Fact]
    public async Task Concurrent_MultiSeat_Holds_Cannot_Exceed_Configured_Capacity()
    {
        var ct = TestContext.Current.CancellationToken;
        await EnsureMigratedAsync(ct);
        var departure = NewDeparture();
        var now = Instant.FromUtc(2026, 8, 18, 8, 10);
        var expires = now.Plus(Duration.FromMinutes(5));
        var seed = await PersistBookingAsync(departure, now, ct);
        await HoldOnNewContextAsync(seed, 3, 5, now, expires, "idem-seed-" + seed.Value, ct);

        var bookingA = await PersistBookingAsync(departure, now, ct);
        var bookingB = await PersistBookingAsync(departure, now, ct);
        var taskA = HoldOnNewContextAsync(bookingA, 2, 5, now, expires, "idem-m-a-" + bookingA.Value, ct);
        var taskB = HoldOnNewContextAsync(bookingB, 2, 5, now, expires, "idem-m-b-" + bookingB.Value, ct);
        var results = await Task.WhenAll(Capture(taskA), Capture(taskB));

        Assert.Equal(1, results.Count(x => x.Hold is not null));
        Assert.Equal(1, results.Count(x => x.Error is InsufficientCapacityException));
        Assert.True(await EffectiveSeatsAsync(departure, ct) <= 5);
        Assert.Equal(5, await EffectiveSeatsAsync(departure, ct));
    }

    [Fact]
    public async Task Release_Frees_Capacity_For_A_Later_Hold()
    {
        var ct = TestContext.Current.CancellationToken;
        await EnsureMigratedAsync(ct);
        var departure = NewDeparture();
        var now = Instant.FromUtc(2026, 8, 18, 8, 20);
        var expires = now.Plus(Duration.FromMinutes(5));
        var firstId = await PersistBookingAsync(departure, now, ct);
        var hold = await HoldOnNewContextAsync(firstId, 1, 1, now, expires, "idem-rel-1-" + firstId.Value, ct);

        await using (var db = _postgres.CreateDbContext())
        {
            await new BookingCapacityService(db).ReleaseAsync(hold.Id, now.Plus(Duration.FromMinutes(1)), ct);
        }

        var secondId = await PersistBookingAsync(departure, now, ct);
        var second = await HoldOnNewContextAsync(secondId, 1, 1, now, expires, "idem-rel-2-" + secondId.Value, ct);
        Assert.Equal(CapacityHoldStatus.Active, second.Status);
        Assert.Equal(1, await EffectiveSeatsAsync(departure, ct));
    }

    [Fact]
    public async Task Expiry_Frees_Capacity_For_A_Later_Hold()
    {
        var ct = TestContext.Current.CancellationToken;
        await EnsureMigratedAsync(ct);
        var departure = NewDeparture();
        var now = Instant.FromUtc(2026, 8, 18, 8, 30);
        var expires = now.Plus(Duration.FromMinutes(5));
        var firstId = await PersistBookingAsync(departure, now, ct);
        var hold = await HoldOnNewContextAsync(firstId, 1, 1, now, expires, "idem-exp-1-" + firstId.Value, ct);

        await using (var db = _postgres.CreateDbContext())
        {
            await new BookingCapacityService(db).ExpireAsync(hold.Id, expires, ct);
        }

        var secondId = await PersistBookingAsync(departure, now, ct);
        var second = await HoldOnNewContextAsync(secondId, 1, 1, now, expires, "idem-exp-2-" + secondId.Value, ct);
        Assert.Equal(CapacityHoldStatus.Active, second.Status);
    }

    [Fact]
    public async Task Consumed_Hold_Still_Consumes_Capacity()
    {
        var ct = TestContext.Current.CancellationToken;
        await EnsureMigratedAsync(ct);
        var departure = NewDeparture();
        var now = Instant.FromUtc(2026, 8, 18, 8, 40);
        var expires = now.Plus(Duration.FromMinutes(5));
        var firstId = await PersistBookingAsync(departure, now, ct);
        var hold = await HoldOnNewContextAsync(firstId, 1, 1, now, expires, "idem-con-1-" + firstId.Value, ct);

        await using (var db = _postgres.CreateDbContext())
        {
            await new BookingCapacityService(db).ConsumeAsync(hold.Id, now.Plus(Duration.FromMinutes(1)), ct);
        }

        var secondId = await PersistBookingAsync(departure, now, ct);
        var captured = await Capture(HoldOnNewContextAsync(secondId, 1, 1, now, expires, "idem-con-2-" + secondId.Value, ct));
        Assert.IsType<InsufficientCapacityException>(captured.Error);
        Assert.Equal(1, await EffectiveSeatsAsync(departure, ct));
    }

    [Fact]
    public async Task Idempotent_Retry_Does_Not_Double_Consume()
    {
        var ct = TestContext.Current.CancellationToken;
        await EnsureMigratedAsync(ct);
        var departure = NewDeparture();
        var now = Instant.FromUtc(2026, 8, 18, 8, 50);
        var expires = now.Plus(Duration.FromMinutes(5));
        var bookingId = await PersistBookingAsync(departure, now, ct);
        var key = "idem-same-" + bookingId.Value;

        var first = await HoldOnNewContextAsync(bookingId, 1, 1, now, expires, key, ct);
        var second = await HoldOnNewContextAsync(bookingId, 1, 1, now, expires, key, ct);

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, await EffectiveSeatsAsync(departure, ct));
        await using var db = _postgres.CreateDbContext();
        Assert.Equal(1, await db.CapacityHolds.CountAsync(x => x.BookingId == bookingId, ct));
    }

    private async Task EnsureMigratedAsync(CancellationToken ct)
    {
        await using var db = _postgres.CreateDbContext();
        await BookingMigrator.MigrateAsync(db, ct);
    }

    private async Task<BookingId> PersistBookingAsync(
        TourDepartureReference departure,
        Instant now,
        CancellationToken ct)
    {
        await using var db = _postgres.CreateDbContext();
        var booking = BookingAggregate.Create(departure, now);
        db.Bookings.Add(booking);
        await db.SaveChangesAsync(ct);
        return booking.Id;
    }

    private async Task<CapacityHold> HoldOnNewContextAsync(
        BookingId bookingId,
        int seats,
        int configured,
        Instant now,
        Instant expires,
        string key,
        CancellationToken ct)
    {
        await using var db = _postgres.CreateDbContext();
        var service = new BookingCapacityService(db);
        return await service.HoldAsync(
            new HoldCapacityCommand(bookingId, seats, configured, expires, now, key),
            ct);
    }

    private async Task<int> EffectiveSeatsAsync(TourDepartureReference departure, CancellationToken ct)
    {
        await using var db = _postgres.CreateDbContext();
        var account = await db.DepartureCapacityAccounts.SingleAsync(x => x.TourDeparture == departure, ct);
        return account.EffectiveSeats;
    }

    private static TourDepartureReference NewDeparture() => new(Guid.CreateVersion7());

    private static async Task<(CapacityHold? Hold, Exception? Error)> Capture(Task<CapacityHold> task)
    {
        try
        {
            return (await task, null);
        }
        catch (Exception ex)
        {
            return (null, ex);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using NodaTime;
using TravelCore.Modules.Ugc.Domain;
using TravelCore.Modules.Ugc.Infrastructure;
using TravelCore.Modules.Ugc.Infrastructure.Services;
using Xunit;

namespace TravelCore.Modules.Ugc.UnitTests;

/// <summary>
/// Admin UGC moderation service (TC-MODOPS-T003).
/// </summary>
public sealed class UgcModerationServiceTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 20, 10, 0);
    private static readonly Instant Later = Instant.FromUtc(2026, 8, 20, 10, 30);
    private static readonly Guid ActorId = Guid.Parse("0198b3e0-0000-7000-8000-0000000000a1");

    [Fact]
    public async Task ListPendingTravelogues_Returns_Pending_Not_Archived()
    {
        await using var db = CreateDb();
        var pending = Travelogue.Create(ActorId, "fa", "Pending title", "Pending body", Now);
        var approved = Travelogue.Create(ActorId, "fa", "Approved title", "Approved body", Now);
        approved.Approve(Later);
        var archived = Travelogue.Create(ActorId, "fa", "Archived title", "Archived body", Now);
        archived.Archive(Later);
        db.Travelogues.AddRange(pending, approved, archived);
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var items = await service.ListPendingTraveloguesAsync(50);

        Assert.Single(items);
        Assert.Equal(pending.Id.Value, items[0].TravelogueId);
        Assert.Equal("Pending", items[0].ModerationStatus);
    }

    [Fact]
    public async Task Approve_Then_Publish_Moves_Travelogue_Out_Of_Pending_Queue()
    {
        await using var db = CreateDb();
        var travelogue = Travelogue.Create(ActorId, "fa", "Title", "Body", Now);
        db.Travelogues.Add(travelogue);
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var approved = await service.ApproveTravelogueAsync(travelogue.Id.Value);
        Assert.Equal("Approved", approved.ModerationStatus);
        Assert.Equal("Draft", approved.PublicationStatus);

        var pendingAfterApprove = await service.ListPendingTraveloguesAsync(50);
        Assert.Empty(pendingAfterApprove);

        var published = await service.PublishTravelogueAsync(travelogue.Id.Value);
        Assert.Equal("Approved", published.ModerationStatus);
        Assert.Equal("Published", published.PublicationStatus);
    }

    [Fact]
    public async Task Reject_Removes_Item_From_Pending_Queue()
    {
        await using var db = CreateDb();
        var travelogue = Travelogue.Create(ActorId, "fa", "Title", "Body", Now);
        db.Travelogues.Add(travelogue);
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var rejected = await service.RejectTravelogueAsync(travelogue.Id.Value);
        Assert.Equal("Rejected", rejected.ModerationStatus);

        var pending = await service.ListPendingTraveloguesAsync(50);
        Assert.Empty(pending);
    }

    [Fact]
    public async Task Publish_Before_Approve_Throws()
    {
        await using var db = CreateDb();
        var travelogue = Travelogue.Create(ActorId, "fa", "Title", "Body", Now);
        db.Travelogues.Add(travelogue);
        await db.SaveChangesAsync();

        var service = CreateService(db);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.PublishTravelogueAsync(travelogue.Id.Value));
    }

    private static UgcModerationService CreateService(UgcDbContext db) =>
        new(db, new FixedClock(Now));

    private sealed class FixedClock(Instant instant) : IClock
    {
        public Instant GetCurrentInstant() => instant;
    }

    private static UgcDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<UgcDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new UgcDbContext(options);
    }
}

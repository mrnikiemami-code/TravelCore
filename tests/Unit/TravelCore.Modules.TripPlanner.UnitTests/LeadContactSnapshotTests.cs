using NodaTime;
using TravelCore.Modules.TripPlanner.Contracts;
using TravelCore.Modules.TripPlanner.Domain;
using Xunit;

namespace TravelCore.Modules.TripPlanner.UnitTests;

/// <summary>
/// Lead submission-time contact snapshot (TC-P18-T003 / P18-R3).
/// </summary>
public sealed class LeadContactSnapshotTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 12, 0);

    [Fact]
    public void Create_Normalizes_Contact_Fields_And_Is_Not_Party_Or_Identity()
    {
        var contact = LeadContactSnapshot.Create(
            "  Sara Ahmadi  ",
            "  Sara@Example.com  ",
            " +98 912 000 0000 ");

        Assert.Equal("Sara Ahmadi", contact.DisplayName);
        Assert.Equal("Sara@Example.com", contact.Email);
        Assert.Equal("SARA@EXAMPLE.COM", contact.NormalizedEmail);
        Assert.Equal("+98 912 000 0000", contact.Phone);
        Assert.Null(typeof(LeadContactSnapshot).GetProperty("PartyId"));
        Assert.Null(typeof(LeadContactSnapshot).GetProperty("AccountId"));
        Assert.Null(typeof(LeadContactSnapshot).GetProperty("CustomerId"));
    }

    [Fact]
    public void Submit_Copies_Contact_Snapshot_Independently_From_Future_Intent_Changes()
    {
        var intent = TripIntent.Create(Now, "Need help");
        var contact = LeadContactSnapshot.Create("Ali", "ali@test.com", "+441234");
        var lead = TripIntentLeadSubmissionBoundary.Submit(intent, Instant.FromUtc(2026, 8, 18, 13, 0), contact);

        intent.UpdatePlanningNote("Changed after submit", Instant.FromUtc(2026, 8, 18, 14, 0));
        intent.AssociateActor(new PlannerActorReference(Guid.Parse("0198b3e0-0000-7000-8000-000000000099")), Instant.FromUtc(2026, 8, 18, 15, 0));

        Assert.Equal("Ali", lead.Contact.DisplayName);
        Assert.Equal("ali@test.com", lead.Contact.Email);
        Assert.Equal("ALI@TEST.COM", lead.Contact.NormalizedEmail);
        Assert.Null(lead.ActorReference);
    }

    [Fact]
    public void Submit_Copies_Optional_Actor_From_Intent_When_Not_Overridden()
    {
        var intent = TripIntent.Create(Now);
        var actorId = Guid.Parse("0198b3e0-0000-7000-8000-000000000052");
        intent.AssociateActor(new PlannerActorReference(actorId), Instant.FromUtc(2026, 8, 18, 12, 30));
        var lead = intent.SubmitAsLead(
            Instant.FromUtc(2026, 8, 18, 13, 0),
            LeadContactSnapshot.Create(email: "traveler@example.com"));

        Assert.Equal(actorId, lead.ActorReference!.Value.ActorId);
    }

    [Fact]
    public void Empty_Contact_Allows_Submission_Without_Forcing_Early_Conversion()
    {
        var intent = TripIntent.Create(Now);
        var lead = intent.SubmitAsLead(Instant.FromUtc(2026, 8, 18, 13, 0));

        Assert.Same(LeadContactSnapshot.Empty, lead.Contact);
        Assert.Null(lead.Contact.Email);
        Assert.Null(lead.Contact.Phone);
    }

    [Fact]
    public void Create_Rejects_Invalid_Email()
    {
        Assert.Throws<ArgumentException>(() => LeadContactSnapshot.Create(email: "not-an-email"));
    }
}

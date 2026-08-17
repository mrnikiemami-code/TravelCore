using NodaTime;
using TravelCore.Modules.TripPlanner.Domain;
using Xunit;

namespace TravelCore.Modules.TripPlanner.UnitTests;

/// <summary>
/// TripIntent aggregate baseline (TC-P18-T002 / P18-R2).
/// </summary>
public sealed class TripIntentTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 10, 0);

    [Fact]
    public void Create_Owns_Mutable_Planning_Context_Not_Lead_Or_Booking()
    {
        var intent = TripIntent.Create(Now, "  Summer family trip  ");

        Assert.NotEqual(Guid.Empty, intent.Id.Value);
        Assert.Equal(1, intent.PlanningRevision);
        Assert.Equal("Summer family trip", intent.PlanningNote);
        Assert.Equal(Now, intent.CreatedAt);
        Assert.Equal(Now, intent.UpdatedAt);
        Assert.NotEqual(typeof(TripIntent), typeof(Lead));
        Assert.Null(typeof(TripIntent).GetProperty("BookingId"));
        Assert.Null(typeof(TripIntent).GetProperty("QuoteId"));
        Assert.Null(typeof(TripIntent).GetProperty("OpportunityId"));
        Assert.Null(typeof(TripIntent).GetProperty("Email"));
        Assert.Null(typeof(TripIntent).GetProperty("PartyId"));
    }

    [Fact]
    public void UpdatePlanningNote_Increments_Revision_And_Keeps_TripIntent_Mutable()
    {
        var intent = TripIntent.Create(Now, "Initial note");
        var later = Instant.FromUtc(2026, 8, 18, 11, 0);
        intent.UpdatePlanningNote("Revised note", later);

        Assert.Equal(2, intent.PlanningRevision);
        Assert.Equal("Revised note", intent.PlanningNote);
        Assert.Equal(later, intent.UpdatedAt);
    }

    [Fact]
    public void SubmitAsLead_Creates_Distinct_Lead_With_Submission_Snapshot()
    {
        var intent = TripIntent.Create(Now, "Need help planning");
        var submittedAt = Instant.FromUtc(2026, 8, 18, 12, 0);
        var lead = intent.SubmitAsLead(submittedAt);

        Assert.NotEqual(intent.Id.Value, lead.Id.Value);
        Assert.Equal(intent.Id, lead.SourceTripIntentId);
        Assert.Equal(LeadStatus.Submitted, lead.Status);
        Assert.Equal(submittedAt, lead.SubmittedAt);
        Assert.Equal(submittedAt, lead.CreatedAt);
        Assert.Equal(1, lead.Snapshot.CapturedPlanningRevision);
        Assert.Equal("Need help planning", lead.Snapshot.CapturedPlanningNote);
        Assert.NotEqual(typeof(TripIntent), typeof(Lead));
    }

    [Fact]
    public void Mutating_TripIntent_After_Submission_Does_Not_Change_Existing_Lead_Snapshot()
    {
        var intent = TripIntent.Create(Now, "Before submit");
        var lead = TripIntentLeadSubmissionBoundary.Submit(intent, Instant.FromUtc(2026, 8, 18, 12, 0));
        intent.UpdatePlanningNote("After submit", Instant.FromUtc(2026, 8, 18, 13, 0));

        Assert.Equal(2, intent.PlanningRevision);
        Assert.Equal("After submit", intent.PlanningNote);
        Assert.Equal(1, lead.Snapshot.CapturedPlanningRevision);
        Assert.Equal("Before submit", lead.Snapshot.CapturedPlanningNote);
    }

    [Fact]
    public void One_TripIntent_May_Produce_Multiple_Leads()
    {
        var intent = TripIntent.Create(Now, "Shared intent");
        var first = TripIntentLeadSubmissionBoundary.Submit(intent, Instant.FromUtc(2026, 8, 18, 12, 0));
        intent.UpdatePlanningNote("Updated between submissions", Instant.FromUtc(2026, 8, 18, 12, 30));
        var second = TripIntentLeadSubmissionBoundary.Submit(intent, Instant.FromUtc(2026, 8, 18, 13, 0));

        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(intent.Id, first.SourceTripIntentId);
        Assert.Equal(intent.Id, second.SourceTripIntentId);
        Assert.Equal("Shared intent", first.Snapshot.CapturedPlanningNote);
        Assert.Equal("Updated between submissions", second.Snapshot.CapturedPlanningNote);
    }
}

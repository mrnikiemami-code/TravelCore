using NodaTime;
using TravelCore.Modules.TripPlanner.Contracts;
using TravelCore.Modules.TripPlanner.Domain;
using TravelCore.Money;
using Xunit;

namespace TravelCore.Modules.TripPlanner.UnitTests;

/// <summary>
/// Lead lifecycle baseline (TC-P18-T005 / P18-R5).
/// </summary>
public sealed class LeadLifecycleTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 10, 0);

    private static Lead CreateSubmittedLead()
    {
        var intent = TripIntent.Create(Now, "Need help");
        intent.UpdatePreferences(
            preferences =>
            {
                preferences.SetTravelers(PlannerTravelerComposition.Create(2));
                preferences.ReplaceDestinations([DestinationPreference.Undecided()]);
            },
            Now);
        return TripIntentLeadSubmissionBoundary.Submit(
            intent,
            Instant.FromUtc(2026, 8, 18, 11, 0),
            LeadContactSnapshot.Create("Ada Lovelace", "ada@example.com", phone: "+1-555-0100"));
    }

    [Fact]
    public void New_Lead_Starts_Submitted()
    {
        var lead = CreateSubmittedLead();

        Assert.Equal(LeadStatus.Submitted, lead.Status);
        Assert.Equal(lead.SubmittedAt, lead.StatusChangedAt);
    }

    [Theory]
    [InlineData(LeadStatus.Contacted)]
    [InlineData(LeadStatus.Closed)]
    [InlineData(LeadStatus.Cancelled)]
    public void Submitted_Transitions_Succeed(LeadStatus target)
    {
        var lead = CreateSubmittedLead();
        var later = Instant.FromUtc(2026, 8, 18, 12, 0);

        ApplyTransition(lead, target, later);

        Assert.Equal(target, lead.Status);
        Assert.Equal(later, lead.StatusChangedAt);
        Assert.Equal(later, lead.UpdatedAt);
    }

    [Theory]
    [InlineData(LeadStatus.Closed)]
    [InlineData(LeadStatus.Cancelled)]
    public void Contacted_Transitions_Succeed(LeadStatus target)
    {
        var lead = CreateSubmittedLead();
        LeadLifecycleBoundary.MarkContacted(lead, Instant.FromUtc(2026, 8, 18, 11, 30));

        ApplyTransition(lead, target, Instant.FromUtc(2026, 8, 18, 12, 0));

        Assert.Equal(target, lead.Status);
    }

    [Fact]
    public void MarkContacted_Is_Idempotent_When_Already_Contacted()
    {
        var lead = CreateSubmittedLead();
        LeadLifecycleBoundary.MarkContacted(lead, Instant.FromUtc(2026, 8, 18, 11, 30));
        var changedAt = lead.StatusChangedAt;

        LeadLifecycleBoundary.MarkContacted(lead, Instant.FromUtc(2026, 8, 18, 12, 0));

        Assert.Equal(LeadStatus.Contacted, lead.Status);
        Assert.Equal(changedAt, lead.StatusChangedAt);
    }

    [Fact]
    public void Closed_To_Contacted_Fails()
    {
        var lead = CreateSubmittedLead();
        LeadLifecycleBoundary.Close(lead, Instant.FromUtc(2026, 8, 18, 11, 30));

        Assert.Throws<InvalidOperationException>(() =>
            LeadLifecycleBoundary.MarkContacted(lead, Instant.FromUtc(2026, 8, 18, 12, 0)));
    }

    [Fact]
    public void Cancelled_To_Submitted_Fails()
    {
        var lead = CreateSubmittedLead();
        LeadLifecycleBoundary.Cancel(lead, Instant.FromUtc(2026, 8, 18, 11, 30));

        Assert.Throws<InvalidOperationException>(() =>
            LeadLifecycleBoundary.MarkContacted(lead, Instant.FromUtc(2026, 8, 18, 12, 0)));
    }

    [Fact]
    public void Status_Transition_Does_Not_Mutate_Submission_Snapshot_Or_Contact()
    {
        var intent = TripIntent.Create(Now, "Before submit");
        intent.UpdatePreferences(
            preferences =>
            {
                preferences.SetBudget(BudgetPreference.Create(CurrencyCode.Parse("USD"), 1000m));
                preferences.ReplaceInterests([InterestPreference.Create("art")]);
            },
            Now);
        var lead = TripIntentLeadSubmissionBoundary.Submit(
            intent,
            Instant.FromUtc(2026, 8, 18, 11, 0),
            LeadContactSnapshot.Create("Contact Name", "contact@example.com"));

        var snapshotNote = lead.Snapshot.CapturedPlanningNote;
        var budget = lead.Snapshot.Preferences.Budget!.MinimumAmount;
        var interest = lead.Snapshot.Preferences.Interests.Single().Code;
        var contactEmail = lead.Contact.Email;

        LeadLifecycleBoundary.MarkContacted(lead, Instant.FromUtc(2026, 8, 18, 12, 0));
        LeadLifecycleBoundary.Close(lead, Instant.FromUtc(2026, 8, 18, 13, 0));

        Assert.Equal(snapshotNote, lead.Snapshot.CapturedPlanningNote);
        Assert.Equal(budget, lead.Snapshot.Preferences.Budget!.MinimumAmount);
        Assert.Equal(interest, lead.Snapshot.Preferences.Interests.Single().Code);
        Assert.Equal(contactEmail, lead.Contact.Email);
    }

    [Fact]
    public void Status_Transition_Does_Not_Mutate_Source_TripIntent()
    {
        var intent = TripIntent.Create(Now, "Mutable intent");
        var lead = TripIntentLeadSubmissionBoundary.Submit(intent, Instant.FromUtc(2026, 8, 18, 11, 0));

        LeadLifecycleBoundary.Close(lead, Instant.FromUtc(2026, 8, 18, 12, 0));
        intent.UpdatePlanningNote("Changed after lead closed", Instant.FromUtc(2026, 8, 18, 13, 0));

        Assert.Equal(LeadStatus.Closed, lead.Status);
        Assert.Equal("Mutable intent", lead.Snapshot.CapturedPlanningNote);
        Assert.Equal("Changed after lead closed", intent.PlanningNote);
    }

    [Fact]
    public void LeadStatus_Is_Controlled_Minimal_Lifecycle_Not_Crm_Pipeline()
    {
        var names = Enum.GetNames(typeof(LeadStatus));
        Assert.Equal(["Submitted", "Contacted", "Closed", "Cancelled"], names);
        Assert.Null(typeof(Lead).GetProperty("OpportunityId"));
        Assert.Null(typeof(Lead).GetProperty("PipelineStage"));
        Assert.Null(typeof(Lead).GetProperty("BookingId"));
        Assert.Equal(TripPlannerLifecycleBoundary.LeadStatusNotEqualCrmPipelineStage, "LeadStatus != CRM Pipeline Stage");
        Assert.Equal(TripPlannerLifecycleBoundary.ContactedNotEqualQualification, "Contacted != Qualification");
        Assert.True(TripPlannerLifecycleBoundary.LeadLifecycleImplemented);
    }

    private static void ApplyTransition(Lead lead, LeadStatus target, Instant now)
    {
        switch (target)
        {
            case LeadStatus.Contacted:
                LeadLifecycleBoundary.MarkContacted(lead, now);
                break;
            case LeadStatus.Closed:
                LeadLifecycleBoundary.Close(lead, now);
                break;
            case LeadStatus.Cancelled:
                LeadLifecycleBoundary.Cancel(lead, now);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(target));
        }
    }
}

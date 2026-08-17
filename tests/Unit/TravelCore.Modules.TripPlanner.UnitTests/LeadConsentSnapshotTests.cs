using NodaTime;
using TravelCore.Modules.TripPlanner.Contracts;
using TravelCore.Modules.TripPlanner.Domain;
using Xunit;

namespace TravelCore.Modules.TripPlanner.UnitTests;

/// <summary>
/// Lead submission-time consent/privacy snapshot (TC-P18-T007 / P18-R7).
/// </summary>
public sealed class LeadConsentSnapshotTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 12, 0);

    [Fact]
    public void Submission_Persists_Consent_Snapshot_Distinct_From_Contact()
    {
        var intent = TripIntent.Create(Now, "Need help");
        var contact = LeadContactSnapshot.Create("Sara", "sara@example.com", "+441234");
        var consent = LeadConsentSnapshot.Create(
            followUpContactAllowed: true,
            marketingAllowed: false,
            privacyNoticeVersion: "P18-PRIVACY-V1",
            preferredContactChannel: LeadContactChannelPreference.Email,
            capturedAt: Instant.FromUtc(2026, 8, 18, 13, 0));
        var lead = TripIntentLeadSubmissionBoundary.Submit(
            intent,
            Instant.FromUtc(2026, 8, 18, 13, 0),
            contact,
            consent);

        Assert.True(lead.Consent.FollowUpContactAllowed);
        Assert.False(lead.Consent.MarketingAllowed);
        Assert.Equal("P18-PRIVACY-V1", lead.Consent.PrivacyNoticeVersion);
        Assert.Equal(LeadContactChannelPreference.Email, lead.Consent.PreferredContactChannel);
        Assert.Equal("sara@example.com", lead.Contact.Email);
        Assert.NotEqual(lead.Contact.GetType(), lead.Consent.GetType());
        Assert.Equal(
            TripPlannerConsentBoundary.LeadContactSnapshotNotEqualLeadConsentSnapshot,
            "LeadContactSnapshot != LeadConsentSnapshot");
    }

    [Fact]
    public void Marketing_Consent_May_Be_False_And_Submission_Remains_Valid()
    {
        var intent = TripIntent.Create(Now);
        var contact = LeadContactSnapshot.Create("Ali", "ali@example.com");
        var consent = LeadConsentSnapshot.Create(
            true,
            marketingAllowed: false,
            "P18-PRIVACY-V1",
            LeadContactChannelPreference.Email,
            Instant.FromUtc(2026, 8, 18, 13, 0));

        var lead = TripIntentLeadSubmissionBoundary.Submit(
            intent,
            Instant.FromUtc(2026, 8, 18, 13, 0),
            contact,
            consent);

        Assert.False(lead.Consent.MarketingAllowed);
        Assert.False(TripPlannerConsentBoundary.MarketingConsentRequiredForLeadSubmission);
    }

    [Fact]
    public void Follow_Up_Permission_Is_Required_When_Contact_Details_Are_Provided()
    {
        var contact = LeadContactSnapshot.Create("No Consent", "no-consent@example.com");
        var consent = LeadConsentSnapshot.Create(
            followUpContactAllowed: false,
            marketingAllowed: false,
            privacyNoticeVersion: "P18-PRIVACY-V1",
            preferredContactChannel: null,
            capturedAt: Now);

        var intent = TripIntent.Create(Now);
        Assert.Throws<InvalidOperationException>(() =>
            TripIntentLeadSubmissionBoundary.Submit(intent, Now, contact, consent));
    }

    [Fact]
    public void Consent_Snapshot_Survives_Independently_From_Future_TripIntent_Changes()
    {
        var intent = TripIntent.Create(Now, "Before submit");
        var consent = LeadConsentSnapshot.Create(
            true,
            true,
            "P18-PRIVACY-V1",
            LeadContactChannelPreference.Email,
            Instant.FromUtc(2026, 8, 18, 13, 0));
        var lead = TripIntentLeadSubmissionBoundary.Submit(
            intent,
            Instant.FromUtc(2026, 8, 18, 13, 0),
            LeadContactSnapshot.Create("Mina", "mina@example.com"),
            consent);

        intent.UpdatePlanningNote("After submit", Instant.FromUtc(2026, 8, 18, 14, 0));

        Assert.True(lead.Consent.MarketingAllowed);
        Assert.Equal("P18-PRIVACY-V1", lead.Consent.PrivacyNoticeVersion);
        Assert.Equal("Before submit", lead.Snapshot.CapturedPlanningNote);
    }

    [Fact]
    public void Lifecycle_Change_Does_Not_Mutate_Consent_Snapshot()
    {
        var intent = TripIntent.Create(Now);
        var lead = TripIntentLeadSubmissionBoundary.Submit(
            intent,
            Instant.FromUtc(2026, 8, 18, 13, 0),
            LeadContactSnapshot.Create("Ops", "ops@example.com"),
            LeadConsentSnapshot.Create(true, false, "P18-PRIVACY-V1", null, Instant.FromUtc(2026, 8, 18, 13, 0)));

        var capturedAt = lead.Consent.CapturedAt;
        var marketing = lead.Consent.MarketingAllowed;

        LeadLifecycleBoundary.Close(lead, Instant.FromUtc(2026, 8, 18, 14, 0));

        Assert.Equal(capturedAt, lead.Consent.CapturedAt);
        Assert.Equal(marketing, lead.Consent.MarketingAllowed);
    }

    [Fact]
    public void Consent_Boundary_Keeps_Notification_And_Marketing_Separate()
    {
        Assert.True(TripPlannerConsentBoundary.ConsentModelImplemented);
        Assert.False(TripPlannerConsentBoundary.NotificationProviderImplemented);
        Assert.False(TripPlannerNotificationBoundary.NotificationProviderImplemented);
        Assert.Equal(TripPlannerConsentBoundary.ConsentNotEqualNotificationDelivery, "Consent != NotificationDelivery");
        Assert.Equal(
            TripPlannerNotificationBoundary.NotificationIntentNotEqualNotificationDelivery,
            "NotificationIntent != NotificationDelivery");
        Assert.Equal(
            TripPlannerConsentBoundary.FollowUpContactAllowedNotEqualAgencyDataSharingPermission,
            "FollowUpContactAllowed != AgencyDataSharingPermission");
        Assert.Null(typeof(LeadConsentSnapshot).GetProperty("SmtpHost"));
        Assert.Null(typeof(LeadConsentSnapshot).GetProperty("PassportNumber"));
    }
}

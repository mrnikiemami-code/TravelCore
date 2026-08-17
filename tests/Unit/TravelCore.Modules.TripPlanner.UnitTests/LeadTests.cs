using NodaTime;
using TravelCore.Modules.TripPlanner.Domain;
using Xunit;

namespace TravelCore.Modules.TripPlanner.UnitTests;

/// <summary>
/// Lead aggregate baseline (TC-P18-T002 / P18-R2).
/// </summary>
public sealed class LeadTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 10, 0);

    [Fact]
    public void Lead_Is_Submitted_Request_Not_Booking_Quote_Or_Crm()
    {
        var intent = TripIntent.Create(Now, "Follow up please");
        var lead = TripIntentLeadSubmissionBoundary.Submit(intent, Instant.FromUtc(2026, 8, 18, 11, 0));

        Assert.Equal(LeadStatus.Submitted, lead.Status);
        Assert.Null(typeof(Lead).GetProperty("BookingId"));
        Assert.Null(typeof(Lead).GetProperty("ReservationId"));
        Assert.Null(typeof(Lead).GetProperty("QuoteId"));
        Assert.Null(typeof(Lead).GetProperty("Price"));
        Assert.Null(typeof(Lead).GetProperty("OpportunityId"));
        Assert.Null(typeof(Lead).GetProperty("SalespersonId"));
        Assert.Null(typeof(Lead).GetProperty("Email"));
        Assert.Null(typeof(Lead).GetProperty("Phone"));
        Assert.Null(typeof(Lead).GetProperty("AgencyId"));
        Assert.Null(typeof(Lead).GetMethod("UpdateSnapshot"));
        Assert.NotEqual(typeof(TripIntent), typeof(Lead));
    }

    [Fact]
    public void LeadStatus_Has_Only_Submitted_Baseline()
    {
        var values = Enum.GetNames(typeof(LeadStatus));
        Assert.Single(values);
        Assert.Equal(nameof(LeadStatus.Submitted), values[0]);
    }
}

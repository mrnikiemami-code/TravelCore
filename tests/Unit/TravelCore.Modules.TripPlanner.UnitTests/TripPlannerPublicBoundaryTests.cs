using TravelCore.Modules.TripPlanner.Contracts;
using TravelCore.Modules.TripPlanner.Domain;
using TravelCore.Modules.TripPlanner.Infrastructure.Services;
using TravelCore.Modules.PublicExperience.Contracts;
using Xunit;

namespace TravelCore.Modules.TripPlanner.UnitTests;

public sealed class TripPlannerPublicBoundaryTests
{
    [Fact]
    public void T008_PublicComposition_Keeps_Honest_Cta_And_No_Search_Booking()
    {
        Assert.True(TripPlannerOwnershipBoundary.PublicExperienceCompositionImplemented);
        Assert.True(TripPlannerOwnershipBoundary.PublicPlannerRouteImplemented);
        Assert.False(TripPlannerPublicCompositionBoundary.PublicExperienceOwnsLeadFacts);
        Assert.False(TripPlannerPublicCompositionBoundary.SearchEngineAllowed);
        Assert.False(TripPlannerPublicCompositionBoundary.BookingCtaAllowed);
        Assert.False(TripPlannerPublicCompositionBoundary.PaymentCtaAllowed);
        Assert.False(TripPlannerPublicCompositionBoundary.CheckoutCtaAllowed);
        Assert.Equal("/plan", TripPlannerPublicCompositionBoundary.PublicRoutePattern);
        Assert.Equal("X-TripPlanner-Draft-Token", TripPlannerPublicCompositionBoundary.DraftTokenHeader);
        Assert.False(PublicExperienceTripPlannerCompositionBoundary.PublicExperienceOwnsLeadFacts);
        Assert.False(PublicExperienceTripPlannerCompositionBoundary.BookingCtaAllowed);
        Assert.Equal("RequestFollowUpOnly", PublicExperienceTripPlannerCompositionBoundary.HonestCtaPosture);
    }

    [Fact]
    public void T008_PreferenceMapper_RoundTrips_Timing_And_Destination()
    {
        var preferences = TravelPreferences.Empty();
        var request = new TripPlannerUpdateIntentRequest(
            PlanningNote: null,
            Timing: new TripPlannerTimingDraft(
                "ExactDates",
                "2026-09-01",
                "2026-09-10",
                null,
                null,
                null,
                null,
                null,
                null),
            Travelers: new TripPlannerTravelersDraft(2, 1, 0),
            Budget: new TripPlannerBudgetDraft(1000m, 2000m, "USD"),
            Accommodation: "Hotel",
            Transport: "Flight",
            TripStyle: "Balanced",
            InterestCodes: ["CULTURE", "FOOD"],
            Destination: new TripPlannerDestinationDraft(
                false,
                [Guid.Parse("0198b3e0-0000-7000-8000-000000000099")]),
            TravelerNote: "Family trip");

        TripPlannerPublicPreferenceMapper.ApplyUpdate(preferences, request);
        var draft = TripPlannerPublicPreferenceMapper.ToDraft(preferences);

        Assert.Equal("ExactDates", draft.Timing.Kind);
        Assert.Equal("2026-09-01", draft.Timing.ExactStartDate);
        Assert.Equal(2, draft.Travelers?.AdultCount);
        Assert.Equal("USD", draft.Budget?.CurrencyCode);
        Assert.Equal("Hotel", draft.Accommodation);
        Assert.False(draft.Destination.Undecided);
        Assert.Single(draft.Destination.LogicalDestinationIds!);
        Assert.Equal(["CULTURE", "FOOD"], draft.InterestCodes);
    }
}

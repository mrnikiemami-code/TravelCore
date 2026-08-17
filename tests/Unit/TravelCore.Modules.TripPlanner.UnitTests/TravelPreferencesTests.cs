using NodaTime;
using TravelCore.Modules.TripPlanner.Contracts;
using TravelCore.Modules.TripPlanner.Domain;
using TravelCore.Money;
using Xunit;

namespace TravelCore.Modules.TripPlanner.UnitTests;

/// <summary>
/// Structured travel preference model (TC-P18-T004 / P18-R4).
/// </summary>
public sealed class TravelPreferencesTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 10, 0);

    [Fact]
    public void TravelTimingPreference_ExactDates_Requires_End_On_Or_After_Start()
    {
        var start = new LocalDate(2026, 9, 1);
        var end = new LocalDate(2026, 9, 10);

        var timing = TravelTimingPreference.Exact(start, end);

        Assert.Equal(TravelTimingKind.ExactDates, timing.Kind);
        Assert.Equal(start, timing.ExactStartDate);
        Assert.Equal(end, timing.ExactEndDate);
        Assert.Throws<ArgumentException>(() => TravelTimingPreference.Exact(end, start));
    }

    [Fact]
    public void PlannerTravelerComposition_Rejects_Negative_Counts()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PlannerTravelerComposition.Create(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => PlannerTravelerComposition.Create(2, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => PlannerTravelerComposition.Create(2, 0, -1));
    }

    [Fact]
    public void BudgetPreference_Is_Total_Trip_Intent_Not_Price_Or_Quote()
    {
        var budget = BudgetPreference.Create(CurrencyCode.Parse("USD"), 1000m, 2500m);

        Assert.Equal(1000m, budget.MinimumAmount);
        Assert.Equal(2500m, budget.MaximumAmount);
        Assert.Equal("USD", budget.CurrencyCode.Value);
        Assert.Null(typeof(BudgetPreference).GetProperty("Price"));
        Assert.Null(typeof(BudgetPreference).GetProperty("QuoteId"));
        Assert.Equal(TripPlannerPreferenceBoundary.BudgetPreferenceNotEqualPrice, "BudgetPreference != Price");
        Assert.Equal(TripPlannerPreferenceBoundary.BudgetPreferenceNotEqualQuote, "BudgetPreference != Quote");
    }

    [Fact]
    public void DestinationPreference_Uses_Logical_Id_Not_Destination_Entity()
    {
        var logicalId = Guid.Parse("0198b3e0-0000-7000-8000-000000000041");
        var destination = DestinationPreference.ForLogicalDestination(logicalId, sortOrder: 1);
        var undecided = DestinationPreference.Undecided(sortOrder: 2);

        Assert.Equal(logicalId, destination.LogicalDestinationId);
        Assert.False(destination.IsUndecided);
        Assert.True(undecided.IsUndecided);
        Assert.Null(typeof(DestinationPreference).GetProperty("DestinationId"));
        Assert.Null(typeof(DestinationPreference).GetProperty("PlaceId"));
    }

    [Fact]
    public void InterestPreference_Uses_Controlled_Code_Not_Search_Facet()
    {
        var interest = InterestPreference.Create("culture");

        Assert.Equal("CULTURE", interest.Code);
        Assert.Throws<ArgumentException>(() => InterestPreference.Create("bad code"));
        Assert.Equal(
            TripPlannerPreferenceBoundary.InterestPreferenceNotEqualSearchFacet,
            "InterestPreference != Search Facet Authority");
    }

    [Fact]
    public void Submit_Copies_Preference_Snapshot_Independent_Of_Later_TripIntent_Mutation()
    {
        var intent = TripIntent.Create(Now);
        var destinationId = Guid.Parse("0198b3e0-0000-7000-8000-000000000042");
        intent.UpdatePreferences(
            preferences =>
            {
                preferences.SetTiming(TravelTimingPreference.Exact(
                    new LocalDate(2026, 10, 1),
                    new LocalDate(2026, 10, 14)));
                preferences.SetTravelers(PlannerTravelerComposition.Create(2, 1));
                preferences.SetBudget(BudgetPreference.Create(CurrencyCode.Parse("EUR"), 1500m, 3000m));
                preferences.SetAccommodation(AccommodationPreferenceKind.Hotel);
                preferences.ReplaceDestinations(
                [
                    DestinationPreference.ForLogicalDestination(destinationId),
                ]);
                preferences.ReplaceInterests([InterestPreference.Create("food")]);
            },
            Instant.FromUtc(2026, 8, 18, 10, 30));

        var lead = TripIntentLeadSubmissionBoundary.Submit(intent, Instant.FromUtc(2026, 8, 18, 11, 0));

        intent.UpdatePreferences(
            preferences =>
            {
                preferences.SetTiming(TravelTimingPreference.Undecided());
                preferences.SetTravelers(null);
                preferences.SetBudget(null);
                preferences.ReplaceDestinations([DestinationPreference.Undecided()]);
                preferences.ReplaceInterests([]);
            },
            Instant.FromUtc(2026, 8, 18, 12, 0));

        var snapshot = lead.Snapshot.Preferences;

        Assert.Equal(TravelTimingKind.ExactDates, snapshot.Timing.Kind);
        Assert.Equal(new LocalDate(2026, 10, 1), snapshot.Timing.ExactStartDate);
        Assert.Equal(2, snapshot.Travelers!.AdultCount);
        Assert.Equal(1, snapshot.Travelers.ChildCount);
        Assert.Equal("EUR", snapshot.Budget!.CurrencyCode.Value);
        Assert.Equal(AccommodationPreferenceKind.Hotel, snapshot.Accommodation);
        Assert.Equal(destinationId, snapshot.Destinations.Single().LogicalDestinationId);
        Assert.Equal("FOOD", snapshot.Interests.Single().Code);
        Assert.Equal(TravelTimingKind.Undecided, intent.Preferences.Timing.Kind);
        Assert.Empty(intent.Preferences.Interests);
    }

    [Fact]
    public void Module_Does_Not_Introduce_BookingPassenger_Entity()
    {
        Assert.Null(typeof(TripPlannerDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.TripPlanner.Domain.BookingPassenger"));
        Assert.Null(typeof(TripPlannerDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.TripPlanner.Domain.Passenger"));
        Assert.NotNull(typeof(TripPlannerDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.TripPlanner.Domain.PlannerTravelerComposition"));
        Assert.Equal(
            TripPlannerPreferenceBoundary.PlannerTravelerCompositionNotEqualBookingPassenger,
            "PlannerTravelerComposition != BookingPassenger");
    }
}

namespace TravelCore.Modules.TripPlanner.Contracts;

/// <summary>
/// P18-R4: TripPlanner preference boundaries vs Booking, Search, Destination, Pricing.
/// </summary>
public static class TripPlannerPreferenceBoundary
{
    public const string PreferenceNotEqualReservationFact = "Preference != Reservation Fact";
    public const string PreferenceNotEqualCatalogFact = "Preference != Catalog Fact";
    public const string DestinationPreferenceNotEqualDestinationSoT = "DestinationPreference != Destination Source of Truth";
    public const string PlannerTravelerCompositionNotEqualBookingPassenger = "PlannerTravelerComposition != BookingPassenger";
    public const string BudgetPreferenceNotEqualPrice = "BudgetPreference != Price";
    public const string BudgetPreferenceNotEqualQuote = "BudgetPreference != Quote";
    public const string InterestPreferenceNotEqualSearchFacet = "InterestPreference != Search Facet Authority";
    public const string AccommodationPreferenceNotEqualHotelInventory = "AccommodationPreference != Hotel Inventory";
    public const string TransportPreferenceNotEqualFlightInventory = "TransportPreference != Flight Inventory";
    public const bool TravelPreferencesImplemented = true;
}

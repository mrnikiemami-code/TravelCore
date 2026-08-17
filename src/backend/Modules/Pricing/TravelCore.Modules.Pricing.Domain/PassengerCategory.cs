namespace TravelCore.Modules.Pricing.Domain;

/// <summary>
/// Pricing-owned passenger commercial categories (TC-P12-T005 baseline).
/// This is not a Booking passenger entity.
/// </summary>
public enum PassengerCategory : short
{
    Adult = 0,
    ChildWithBed = 1,
    ChildWithoutBed = 2
}

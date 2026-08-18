namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// P19-R4 people/PII boundary. Transaction-time snapshots only.
/// BirthDate omitted: traveler category is explicit, not age-inferred.
/// Infant seat-consumption special handling is DEFERRED; PassengerCount counts every passenger.
/// </summary>
public static class BookingPeopleBoundary
{
    public const string PlannerTravelerCompositionIsNotBookingPassenger = "PlannerTravelerComposition != BookingPassenger";
    public const string BookingPassengerIsNotPartyPersonMaster = "BookingPassenger != Party Person Master";
    public const string BookingContactSnapshotIsNotParty = "BookingContactSnapshot != Party";
    public const string BookingContactSnapshotIsNotIdentityAccount = "BookingContactSnapshot != Identity Account";
    public const string BookingPassengerIsNotCapacityHold = "BookingPassenger != CapacityHold";
    public const string BookingPassengerIsNotVisaApplication = "BookingPassenger != VisaApplication";
    public const string BookingPassengerIsNotTravelDocument = "BookingPassenger != TravelDocument";
    public const string PassengerPiiIsNotPublicSearch = "Passenger PII != public Search/SEO data";
    public const string BookingPiiIsNotSearchSeoData = "Booking PII != Search/SEO data";
    public const string PiiRetention = "future explicit operational/legal policy";
    public const bool BirthDateImplemented = false;
    public const bool PassportImplemented = false;
    public const bool DocumentUploadImplemented = false;
    public const bool InfantSeatPolicyImplemented = false;
    public const bool PostConfirmationPassengerAmendmentImplemented = false;
    public const bool AllowPassengerCountAtMostHeldSeats = true;
}

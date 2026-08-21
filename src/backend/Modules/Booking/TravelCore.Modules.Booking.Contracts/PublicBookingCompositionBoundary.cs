namespace TravelCore.Modules.Booking.Contracts;

/// <summary>
/// Public Tour Booking initiation / authorized reads / privacy (TC-P19-T008 / P19-R8).
/// PublicExperience composes presentation only. Booking remains transactional SoT.
/// Initiation creates Pending Booking — it does not confirm, pay, or cancel.
/// </summary>
public static class PublicBookingCompositionBoundary
{
    public const string PublicApiGroup = "/api/booking/public";
    public const string AccessTokenHeader = "X-TravelCore-Booking-Access-Token";
    public const string IdempotencyHeader = "Idempotency-Key";
    public const string ActorAccountIdClaimType = "tc_account_id";
    public const string PublicExperienceIsNotBookingSourceOfTruth = "PublicExperience != Booking Source of Truth";
    public const string PublicInitiationIsNotConfirmation = "Public Booking initiation != Booking confirmation";
    public const string PendingIsNotConfirmed = "Pending != Confirmed";
    public const string BookingIdIsNotAccessCredential = "BookingId != Access Credential";
    public const bool PublicCancellationImplemented = false;
    public const bool PublicListingImplemented = false;
    public const bool ConfirmEndpointImplemented = false;
    public const bool PaymentEndpointImplemented = true;
    public const bool AgencyOriginOnPublicInitiationImplemented = true;
}

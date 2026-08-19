namespace TravelCore.Modules.Analytics.Domain;

/// <summary>
/// P27-R4 privacy/PII interaction boundary marker. Analytics references opaque ids only.
/// </summary>
public static class AnalyticsPrivacyBoundary
{
    public const string AnalyticsIsNotPiiSoR = "Analytics must not become PII SoR";
    public const string OpaqueReferencePosture = "Opaque resource/session references only";
    public const string BookingPartyIdentitySoR = "Booking/Party remain identity SoR";
    public const string NoEmailPhoneInAnalyticsPayload = "No raw email/phone in analytics payload by default";

    public const bool PrivacyBoundaryImplemented = true;
    public const bool PiiPersistenceImplemented = false;
    public const bool IdentityGraphImplemented = false;
    public const bool CrossModulePiiDuplicationImplemented = false;
}

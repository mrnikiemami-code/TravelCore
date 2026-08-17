namespace TravelCore.Modules.Visa.Contracts;

/// <summary>
/// P17-R1: Visa is the independent visa-domain owner (schema <c>visa</c>).
/// Not Destination/ReferenceData geography, not Content CMS, not MediaAsset SoT,
/// not Pricing/Quote, not Booking/Payment, not SEO IndexPolicy, not Search.
/// </summary>
public static class VisaOwnershipBoundary
{
    public const string OwnerModule = "Visa";
    public const string SchemaName = "visa";
    public const string GeographicOwner = "Destination";
    public const string ReferenceDataOwner = "ReferenceData";
    public const string EditorialOwner = "Content";
    public const string MediaAssetOwner = "Media";
    public const string PriceOwner = "Pricing";
    public const string IndexPolicyOwner = "Seo";
    public const string SearchOwner = "Search";
    public const string BookingOwner = "Booking";
    public const string PaymentOwner = "Payment";
    public const string GeographicReferencePosture = "OpaqueLogicalGeographicId";
    public const bool OwnsDestinationFacts = false;
    public const bool OwnsReferenceData = false;
    public const bool OwnsContentCms = false;
    public const bool OwnsMediaAssetTruth = false;
    public const bool OwnsPricing = false;
    public const bool OwnsQuote = false;
    public const bool OwnsBooking = false;
    public const bool OwnsPayment = false;
    public const bool OwnsIndexPolicy = false;
    public const bool OwnsSearch = false;
    public const bool OwnsIdentityOrParty = false;
    public const bool GeographicReferencesAreLogicalOnly = true;
    public const bool GeographicReferencesAreSourceOfTruth = false;
    public const bool FutureEffectivePeriodAllowed = true;
    public const bool FutureProvenanceAllowed = true;
    public const bool FutureVerificationTimestampAllowed = true;
    public const bool FutureJurisdictionContextAllowed = true;
    public const bool RegulatoryEngineImplemented = false;
    public const bool VisaDefinitionImplemented = true;
    public const bool VisaRequirementSetImplemented = true;
    public const bool VisaRequirementImplemented = false;
    public const bool RequiredDocumentImplemented = false;
    public const bool EligibilityModelImplemented = false;
    public const bool ProcessingValidityModelImplemented = false;
    public const bool FeeModelImplemented = false;
    public const bool ApplicationWorkflowImplemented = false;
}

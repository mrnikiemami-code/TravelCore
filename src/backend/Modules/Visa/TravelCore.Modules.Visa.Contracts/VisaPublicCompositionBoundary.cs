namespace TravelCore.Modules.Visa.Contracts;

/// <summary>
/// P17-R7: Visa is the structured visa-fact owner. PublicExperience composes presentation.
/// Content remains editorial. SEO owns IndexPolicy. Search remains retrieval owner.
/// </summary>
public static class VisaPublicCompositionBoundary
{
    public const string FactOwner = "Visa";
    public const string PresentationOwner = "PublicExperience";
    public const string EditorialOwner = "Content";
    public const string IndexPolicyOwner = "Seo";
    public const string SearchOwner = "Search";
    public const string RetrievalPosture = "DeterministicReplaceableQuery";
    public const string PublicRoutePattern = "/visas/{code}";
    public const bool PublicPresenceEqualsSeoIndexed = false;
    public const bool PublicPresenceEqualsAutomaticallySearchIndexed = false;
    public const bool CopyContentIntoVisaAllowed = false;
    public const bool VisaOwnsIndexPolicy = false;
    public const bool VisaOwnsSearch = false;
    public const bool ApplicationWorkflowAllowed = false;
    public const bool CommercialPriceDisplayAllowed = false;
    public const bool FxConversionAllowed = false;
    public const bool DocumentUploadAllowed = false;
    public const bool AppointmentBookingAllowed = false;
    public const bool PaymentCtaAllowed = false;
    public const bool PrivateCaseDataExposureAllowed = false;
}

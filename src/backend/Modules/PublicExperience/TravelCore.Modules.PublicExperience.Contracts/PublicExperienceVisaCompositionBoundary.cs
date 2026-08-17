namespace TravelCore.Modules.PublicExperience.Contracts;

/// <summary>
/// P17-R7: Public VisaDetailPage is presentation/composition only.
/// Visa remains structured-fact SoT. Content remains editorial. SEO owns IndexPolicy.
/// Public Visa page != automatically SEO indexed. Not a Search engine and not an application workflow.
/// </summary>
public static class PublicExperienceVisaCompositionBoundary
{
    public const string PresentationOwner = "PublicExperience";
    public const string FactOwner = "Visa";
    public const string EditorialOwner = "Content";
    public const string IndexPolicyOwner = "Seo";
    public const string FutureRetrievalOwner = "Search";
    public const string RetrievalPosture = "DeterministicReplaceableQuery";
    public const string PublicRoutePattern = "/visas/{code}";
    public const bool CopyContentIntoVisaAllowed = false;
    public const bool PublicPresenceEqualsSeoIndexed = false;
    public const bool PublicPresenceEqualsAutomaticallySearchIndexed = false;
    public const bool VisaOwnsIndexPolicy = false;
    public const bool SearchEngineAllowed = false;
    public const bool ApplicationWorkflowAllowed = false;
    public const bool CommercialPriceDisplayAllowed = false;
    public const bool BookingCtaAllowed = false;
}

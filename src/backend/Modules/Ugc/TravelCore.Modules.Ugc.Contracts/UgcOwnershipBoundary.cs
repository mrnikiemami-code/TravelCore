namespace TravelCore.Modules.Ugc.Contracts;

/// <summary>
/// P16-R1: UGC is the independent user-generated content owner (schema <c>ugc</c>).
/// Not Identity/Party, not Content CMS, not MediaAsset SoT, not target-domain owner,
/// not SEO IndexPolicy, not Search, not Booking/Payment.
/// </summary>
public static class UgcOwnershipBoundary
{
    public const string OwnerModule = "Ugc";
    public const string SchemaName = "ugc";
    public const string IdentityOwner = "Identity";
    public const string PartyOwner = "Party";
    public const string EditorialOwner = "Content";
    public const string MediaAssetOwner = "Media";
    public const string IndexPolicyOwner = "Seo";
    public const string SearchOwner = "Search";
    public const string ActorReferencePosture = "OpaqueLogicalActorId";
    public const bool OwnsIdentityOrParty = false;
    public const bool OwnsContentCms = false;
    public const bool OwnsMediaAssetTruth = false;
    public const bool OwnsTourFacts = false;
    public const bool OwnsPlaceFacts = false;
    public const bool OwnsDestinationFacts = false;
    public const bool OwnsIndexPolicy = false;
    public const bool OwnsSearch = false;
    public const bool OwnsBooking = false;
    public const bool OwnsPayment = false;
    public const bool OwnsTargetFacts = false;
    public const bool ReviewTargetIsLogicalReferenceOnly = true;
    public const bool TravelogueIsNotContentItem = true;
    public const bool ReviewImplemented = true;
    public const bool RatingImplemented = false;
    public const bool RatingIsIndependentAggregate = false;
    public const bool OverallRatingOwnedByReview = true;
    public const bool DimensionRatingsAreReviewChildren = true;
    public const bool TravelogueImplemented = true;
    public const bool UserPhotoImplemented = true;
    public const bool UserPhotoIsNotMediaAsset = true;
    public const bool CommentImplemented = true;
    public const bool LikeImplemented = false;
    public const bool LikeDeferred = true;
    public const bool ReportImplemented = true;
    public const bool ModerationWorkflowImplemented = true;
    public const bool ApprovedEqualsPublished = false;
    public const bool PublishedEqualsSeoIndexed = false;
    public const bool ReportTriggersAutomaticEnforcement = false;
    public const bool TargetAttachmentModelCommitted = true;
    public const bool PublicReadContractsImplemented = true;
    public const bool PubliclyEligibleEqualsSeoIndexed = false;
    public const bool PubliclyEligibleEqualsAutomaticallySearchIndexed = false;
    public const bool IndependentAverageRatingEngineAllowed = false;
    public const bool RatingSummaryIsDerivedRebuildable = true;
    public const bool SearchEngineInUgcAllowed = false;
    public const bool UgcOwnedSeoPagesAllowed = false;
}

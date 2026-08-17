using TravelCore.Modules.Ugc.Contracts;
using TravelCore.Modules.Ugc.Domain;
using TravelCore.Modules.Ugc.Infrastructure;
using Xunit;

namespace TravelCore.Modules.Ugc.UnitTests;

public sealed class UgcScaffoldingSmokeTests
{
    [Fact]
    public void UgcContractsAssembly_IsLoadable()
    {
        var marker = typeof(UgcContractsAssemblyMarker);
        Assert.Equal("TravelCore.Modules.Ugc.Contracts", marker.Namespace);
        Assert.Equal("TravelCore.Modules.Ugc.Contracts", marker.Assembly.GetName().Name);
    }

    [Fact]
    public void UgcDomainAssembly_IsLoadable()
    {
        var marker = typeof(UgcDomainAssemblyMarker);
        Assert.Equal("TravelCore.Modules.Ugc.Domain", marker.Namespace);
    }

    [Fact]
    public void OwnershipBoundary_Keeps_Peer_SoT_Out_Of_Ugc()
    {
        Assert.Equal("Ugc", UgcOwnershipBoundary.OwnerModule);
        Assert.Equal("ugc", UgcOwnershipBoundary.SchemaName);
        Assert.Equal("Identity", UgcOwnershipBoundary.IdentityOwner);
        Assert.Equal("Party", UgcOwnershipBoundary.PartyOwner);
        Assert.Equal("Content", UgcOwnershipBoundary.EditorialOwner);
        Assert.Equal("Media", UgcOwnershipBoundary.MediaAssetOwner);
        Assert.Equal("Seo", UgcOwnershipBoundary.IndexPolicyOwner);
        Assert.Equal("Search", UgcOwnershipBoundary.SearchOwner);
        Assert.Equal("OpaqueLogicalActorId", UgcOwnershipBoundary.ActorReferencePosture);
        Assert.False(UgcOwnershipBoundary.OwnsIdentityOrParty);
        Assert.False(UgcOwnershipBoundary.OwnsContentCms);
        Assert.False(UgcOwnershipBoundary.OwnsMediaAssetTruth);
        Assert.False(UgcOwnershipBoundary.OwnsTourFacts);
        Assert.False(UgcOwnershipBoundary.OwnsPlaceFacts);
        Assert.False(UgcOwnershipBoundary.OwnsDestinationFacts);
        Assert.False(UgcOwnershipBoundary.OwnsIndexPolicy);
        Assert.False(UgcOwnershipBoundary.OwnsSearch);
        Assert.False(UgcOwnershipBoundary.OwnsBooking);
        Assert.False(UgcOwnershipBoundary.OwnsPayment);
        Assert.True(UgcOwnershipBoundary.ReviewImplemented);
        Assert.False(UgcOwnershipBoundary.RatingImplemented);
        Assert.False(UgcOwnershipBoundary.RatingIsIndependentAggregate);
        Assert.True(UgcOwnershipBoundary.OverallRatingOwnedByReview);
        Assert.True(UgcOwnershipBoundary.DimensionRatingsAreReviewChildren);
        Assert.True(UgcOwnershipBoundary.TravelogueImplemented);
        Assert.True(UgcOwnershipBoundary.TravelogueIsNotContentItem);
        Assert.True(UgcOwnershipBoundary.UserPhotoImplemented);
        Assert.True(UgcOwnershipBoundary.UserPhotoIsNotMediaAsset);
        Assert.True(UgcOwnershipBoundary.CommentImplemented);
        Assert.False(UgcOwnershipBoundary.LikeImplemented);
        Assert.True(UgcOwnershipBoundary.LikeDeferred);
        Assert.False(UgcOwnershipBoundary.ReportImplemented);
        Assert.False(UgcOwnershipBoundary.ModerationWorkflowImplemented);
        Assert.True(UgcOwnershipBoundary.TargetAttachmentModelCommitted);
        Assert.True(UgcOwnershipBoundary.ReviewTargetIsLogicalReferenceOnly);
        Assert.False(UgcOwnershipBoundary.OwnsTargetFacts);
    }

    [Fact]
    public void ActorReference_Is_Opaque_Logical_Id_Not_A_User_Entity()
    {
        var actorId = Guid.Parse("0198b3e0-0000-7000-8000-000000000021");
        var reference = new UgcActorReference(actorId);
        Assert.Equal(actorId, reference.ActorId);
        Assert.Equal("UgcActorReference", nameof(UgcActorReference));
        Assert.False(typeof(UgcActorReference).IsClass);
    }

    [Fact]
    public void UgcDbContext_Owns_Schema_ugc()
    {
        Assert.Equal("ugc", UgcDbContext.SchemaName);
        Assert.Equal(UgcOwnershipBoundary.SchemaName, UgcDbContext.SchemaName);
    }
}

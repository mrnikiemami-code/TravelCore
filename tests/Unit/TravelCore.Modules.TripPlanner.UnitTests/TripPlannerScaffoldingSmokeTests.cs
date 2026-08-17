using TravelCore.Modules.TripPlanner.Contracts;
using TravelCore.Modules.TripPlanner.Domain;
using TravelCore.Modules.TripPlanner.Infrastructure;
using Xunit;

namespace TravelCore.Modules.TripPlanner.UnitTests;

public sealed class TripPlannerScaffoldingSmokeTests
{
    [Fact]
    public void TripPlannerContractsAssembly_IsLoadable()
    {
        var marker = typeof(TripPlannerContractsAssemblyMarker);
        Assert.Equal("TravelCore.Modules.TripPlanner.Contracts", marker.Namespace);
        Assert.Equal("TravelCore.Modules.TripPlanner.Contracts", marker.Assembly.GetName().Name);
    }

    [Fact]
    public void TripPlannerDomainAssembly_IsLoadable()
    {
        var marker = typeof(TripPlannerDomainAssemblyMarker);
        Assert.Equal("TravelCore.Modules.TripPlanner.Domain", marker.Namespace);
    }

    [Fact]
    public void OwnershipBoundary_Keeps_Peer_SoT_Out_Of_TripPlanner()
    {
        Assert.Equal("TripPlanner", TripPlannerOwnershipBoundary.OwnerModule);
        Assert.Equal("trip_planner", TripPlannerOwnershipBoundary.SchemaName);
        Assert.Equal("Destination", TripPlannerOwnershipBoundary.DestinationOwner);
        Assert.Equal("ReferenceData", TripPlannerOwnershipBoundary.ReferenceDataOwner);
        Assert.Equal("Tour", TripPlannerOwnershipBoundary.TourOwner);
        Assert.Equal("Place", TripPlannerOwnershipBoundary.PlaceOwner);
        Assert.Equal("Pricing", TripPlannerOwnershipBoundary.PricingOwner);
        Assert.Equal("AgencyMarketplace", TripPlannerOwnershipBoundary.AgencyMarketplaceOwner);
        Assert.Equal("Search", TripPlannerOwnershipBoundary.SearchOwner);
        Assert.Equal("Booking", TripPlannerOwnershipBoundary.BookingOwner);
        Assert.Equal("Payment", TripPlannerOwnershipBoundary.PaymentOwner);
        Assert.Equal("Notification", TripPlannerOwnershipBoundary.NotificationOwner);
        Assert.Equal("Identity", TripPlannerOwnershipBoundary.IdentityOwner);
        Assert.Equal("Party", TripPlannerOwnershipBoundary.PartyOwner);
        Assert.Equal("Crm", TripPlannerOwnershipBoundary.CrmOwner);
        Assert.Equal("PublicExperience", TripPlannerOwnershipBoundary.PresentationOwner);
        Assert.Equal("OpaqueLogicalReferenceId", TripPlannerOwnershipBoundary.LogicalReferencePosture);
        Assert.False(TripPlannerOwnershipBoundary.OwnsDestinationFacts);
        Assert.False(TripPlannerOwnershipBoundary.OwnsReferenceData);
        Assert.False(TripPlannerOwnershipBoundary.OwnsTourFacts);
        Assert.False(TripPlannerOwnershipBoundary.OwnsPlaceFacts);
        Assert.False(TripPlannerOwnershipBoundary.OwnsPricing);
        Assert.False(TripPlannerOwnershipBoundary.OwnsQuote);
        Assert.False(TripPlannerOwnershipBoundary.OwnsBooking);
        Assert.False(TripPlannerOwnershipBoundary.OwnsPayment);
        Assert.False(TripPlannerOwnershipBoundary.OwnsCrm);
        Assert.False(TripPlannerOwnershipBoundary.OwnsSearch);
        Assert.False(TripPlannerOwnershipBoundary.OwnsAgencyMarketplace);
        Assert.False(TripPlannerOwnershipBoundary.OwnsNotificationDelivery);
        Assert.False(TripPlannerOwnershipBoundary.OwnsIdentityOrParty);
        Assert.True(TripPlannerOwnershipBoundary.ProductReferencesAreLogicalOnly);
        Assert.False(TripPlannerOwnershipBoundary.ProductReferencesAreSourceOfTruth);
        Assert.True(TripPlannerOwnershipBoundary.TripIntentImplemented);
        Assert.True(TripPlannerOwnershipBoundary.LeadImplemented);
        Assert.True(TripPlannerOwnershipBoundary.AnonymousTripIntentSupported);
        Assert.True(TripPlannerOwnershipBoundary.AuthenticatedAssociationOptional);
        Assert.True(TripPlannerOwnershipBoundary.LeadContactSnapshotImplemented);
        Assert.False(TripPlannerOwnershipBoundary.IdentityOrPartyCloneImplemented);
        Assert.True(TripPlannerIdentityBoundary.AnonymousTripIntentSupported);
        Assert.True(TripPlannerIdentityBoundary.AuthenticatedAssociationOptional);
        Assert.False(TripPlannerIdentityBoundary.IdentityAuthorityDuplicated);
        Assert.False(TripPlannerIdentityBoundary.PartyMasterDuplicated);
        Assert.False(TripPlannerIdentityBoundary.PersistentAnonymousUserPlatform);
        Assert.True(TripPlannerIdentityBoundary.LeadContactSnapshotImplemented);
        Assert.False(TripPlannerIdentityBoundary.ConsentModelImplemented);
        Assert.False(TripPlannerOwnershipBoundary.TravelPreferencesImplemented);
        Assert.False(TripPlannerOwnershipBoundary.LeadLifecycleImplemented);
        Assert.False(TripPlannerOwnershipBoundary.AgencyRoutingImplemented);
        Assert.False(TripPlannerOwnershipBoundary.ConsentModelImplemented);
        Assert.False(TripPlannerOwnershipBoundary.NotificationProviderImplemented);
        Assert.False(TripPlannerOwnershipBoundary.SearchEngineImplemented);
        Assert.False(TripPlannerOwnershipBoundary.RecommendationEngineImplemented);
        Assert.False(TripPlannerOwnershipBoundary.AiInfrastructureImplemented);
        Assert.False(TripPlannerOwnershipBoundary.GenericWorkflowEngineImplemented);
    }

    [Fact]
    public void LogicalReference_Is_Opaque_Logical_Id_Not_A_Destination_Entity()
    {
        var logicalId = Guid.Parse("0198b3e0-0000-7000-8000-000000000041");
        var reference = new TripPlannerLogicalReference(logicalId);
        Assert.Equal(logicalId, reference.LogicalId);
        Assert.Equal("TripPlannerLogicalReference", nameof(TripPlannerLogicalReference));
        Assert.False(typeof(TripPlannerLogicalReference).IsClass);
    }

    [Fact]
    public void TripPlannerDbContext_Owns_Schema_trip_planner()
    {
        Assert.Equal("trip_planner", TripPlannerDbContext.SchemaName);
        Assert.Equal(TripPlannerOwnershipBoundary.SchemaName, TripPlannerDbContext.SchemaName);
    }
}

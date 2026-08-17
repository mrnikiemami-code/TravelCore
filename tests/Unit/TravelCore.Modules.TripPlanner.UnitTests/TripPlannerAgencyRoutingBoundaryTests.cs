using TravelCore.Modules.TripPlanner.Contracts;
using TravelCore.Modules.TripPlanner.Domain;
using Xunit;

namespace TravelCore.Modules.TripPlanner.UnitTests;

/// <summary>
/// Agency routing boundary posture (TC-P18-T006 / P18-R6 DEFERRED).
/// </summary>
public sealed class TripPlannerAgencyRoutingBoundaryTests
{
    [Fact]
    public void Agency_Routing_Is_Explicitly_Deferred_Without_Assignment_Product_Model()
    {
        Assert.True(TripPlannerAgencyRoutingBoundary.AgencyRoutingDecisionResolved);
        Assert.False(TripPlannerAgencyRoutingBoundary.AgencyRoutingProductImplemented);
        Assert.False(TripPlannerOwnershipBoundary.AgencyRoutingImplemented);
        Assert.True(TripPlannerOwnershipBoundary.AgencyRoutingDecisionResolved);
        Assert.Equal(TripPlannerAgencyRoutingBoundary.AgencyRoutingDeferred, "P18 Agency Routing = DEFERRED");
        Assert.Equal(TripPlannerAgencyRoutingBoundary.LeadNotEqualAgencyAssignment, "Lead != AgencyAssignment");
        Assert.Equal(TripPlannerAgencyRoutingBoundary.LeadStatusNotEqualAgencyAssignmentStatus, "LeadStatus != AgencyAssignmentStatus");

        Assert.Null(typeof(Lead).GetProperty("AssignedAgencyId"));
        Assert.Null(typeof(Lead).GetProperty("PrimaryAgencyId"));
        Assert.Null(typeof(Lead).GetProperty("AgencyOwnerId"));
        Assert.Null(typeof(Lead).GetProperty("AssignmentStatus"));
        Assert.Null(typeof(TripPlannerDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.TripPlanner.Domain.AgencyAssignment"));
        Assert.Null(typeof(TripPlannerDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.TripPlanner.Domain.LeadAssignment"));
    }
}

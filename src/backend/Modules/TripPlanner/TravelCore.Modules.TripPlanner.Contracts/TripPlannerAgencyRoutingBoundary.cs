namespace TravelCore.Modules.TripPlanner.Contracts;

/// <summary>
/// P18-R6: Agency routing/assignment is explicitly DEFERRED from TripPlanner product implementation.
/// </summary>
public static class TripPlannerAgencyRoutingBoundary
{
    public const string LeadNotEqualAgencyAssignment = "Lead != AgencyAssignment";
    public const string TripPlannerNotEqualAgencyMarketplace = "TripPlanner != AgencyMarketplace";
    public const string TripPlannerNotEqualAgencyRankingAuthority = "TripPlanner != Agency Ranking Authority";
    public const string TripPlannerNotEqualCommercialAllocationAuthority = "TripPlanner != Commercial Allocation Authority";
    public const string LeadStatusNotEqualAgencyAssignmentStatus = "LeadStatus != AgencyAssignmentStatus";
    public const string LeadRoutingNotEqualSearchRanking = "Lead Routing != Search Ranking";
    public const string LeadRoutingNotEqualRecommendation = "Lead Routing != Recommendation";
    public const string BudgetPreferenceNotEqualLeadCommercialValue = "BudgetPreference != Lead Commercial Value";
    public const string AgencyRoutingDeferred = "P18 Agency Routing = DEFERRED";
    public const bool AgencyRoutingProductImplemented = false;
    public const bool AgencyRoutingDecisionResolved = true;
}

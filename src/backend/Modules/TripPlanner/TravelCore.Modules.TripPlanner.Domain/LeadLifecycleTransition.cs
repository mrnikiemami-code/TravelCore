namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Deterministic Lead lifecycle transitions (P18-R5). Not a generic workflow engine.
/// </summary>
internal static class LeadLifecycleTransition
{
    internal static bool CanTransition(LeadStatus current, LeadStatus target)
    {
        if (current == target)
        {
            return true;
        }

        return (current, target) switch
        {
            (LeadStatus.Submitted, LeadStatus.Contacted) => true,
            (LeadStatus.Submitted, LeadStatus.Closed) => true,
            (LeadStatus.Submitted, LeadStatus.Cancelled) => true,
            (LeadStatus.Contacted, LeadStatus.Closed) => true,
            (LeadStatus.Contacted, LeadStatus.Cancelled) => true,
            _ => false,
        };
    }

    internal static bool IsTerminal(LeadStatus status) =>
        status is LeadStatus.Closed or LeadStatus.Cancelled;
}

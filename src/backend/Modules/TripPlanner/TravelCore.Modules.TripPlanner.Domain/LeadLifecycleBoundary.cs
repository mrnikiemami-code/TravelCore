using NodaTime;

namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Domain lifecycle commands for Lead (TC-P18-T005 / P18-R5).
/// Behavior-oriented commands; not generic SetStatus(anything).
/// </summary>
public static class LeadLifecycleBoundary
{
    public static void MarkContacted(Lead lead, Instant now) =>
        Transition(lead, LeadStatus.Contacted, now);

    public static void Close(Lead lead, Instant now) =>
        Transition(lead, LeadStatus.Closed, now);

    public static void Cancel(Lead lead, Instant now) =>
        Transition(lead, LeadStatus.Cancelled, now);

    private static void Transition(Lead lead, LeadStatus targetStatus, Instant now)
    {
        ArgumentNullException.ThrowIfNull(lead);
        if (now == default)
        {
            throw new ArgumentException("UpdatedAt cannot be default.", nameof(now));
        }

        if (lead.Status == targetStatus)
        {
            return;
        }

        if (!LeadLifecycleTransition.CanTransition(lead.Status, targetStatus))
        {
            throw new InvalidOperationException(
                $"Lead lifecycle transition {lead.Status} -> {targetStatus} is not allowed.");
        }

        lead.ApplyStatusChange(targetStatus, now);
    }
}

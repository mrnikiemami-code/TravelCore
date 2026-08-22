namespace TravelCore.Modules.CommercialFinance.Domain;

/// <summary>
/// Safe Commercial Obligation lifecycle transition guards (P39-T003).
/// No automatic Payment/Booking event handlers — callers must invoke explicitly.
/// </summary>
public static class CommercialObligationLifecycleRules
{
    public static bool CanTransition(
        CommercialObligationLifecycleState from,
        CommercialObligationLifecycleState to)
    {
        if (from == to)
        {
            return false;
        }

        return from switch
        {
            CommercialObligationLifecycleState.Created => to is
                CommercialObligationLifecycleState.Pending
                or CommercialObligationLifecycleState.Cancelled,
            CommercialObligationLifecycleState.Pending => to is
                CommercialObligationLifecycleState.Approved
                or CommercialObligationLifecycleState.Cancelled,
            CommercialObligationLifecycleState.Approved => to is
                CommercialObligationLifecycleState.Settled
                or CommercialObligationLifecycleState.Cancelled,
            CommercialObligationLifecycleState.Settled => to is
                CommercialObligationLifecycleState.Reversed,
            CommercialObligationLifecycleState.Cancelled => false,
            CommercialObligationLifecycleState.Reversed => false,
            _ => false,
        };
    }

    public static void EnsureCanTransition(
        CommercialObligationLifecycleState from,
        CommercialObligationLifecycleState to)
    {
        if (!CanTransition(from, to))
        {
            throw new InvalidOperationException(
                $"Commercial obligation lifecycle transition {from} -> {to} is not allowed.");
        }
    }

    public static bool IsTerminal(CommercialObligationLifecycleState state) =>
        state is CommercialObligationLifecycleState.Cancelled
            or CommercialObligationLifecycleState.Reversed;
}

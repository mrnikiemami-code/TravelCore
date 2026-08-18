using NodaTime;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Immutable structured fare-rule facts for an accepted Flight offer.
/// Not cancellation execution and not Refund. TicketingDeadline is distinct from OfferExpiresAt.
/// </summary>
public sealed class FlightFareRulesSnapshot
{
    private readonly List<FlightBaggageAllowanceSnapshot> _baggage = [];

    private FlightFareRulesSnapshot()
    {
    }

    internal FlightFareRulesSnapshot(
        FlightOfferSnapshotId flightOfferSnapshotId,
        bool refundable,
        bool changeable,
        Instant? ticketingDeadline,
        MoneyValue? cancelPenalty,
        MoneyValue? changePenalty,
        bool partialRefundRequired)
    {
        FlightOfferSnapshotId = flightOfferSnapshotId;
        Refundable = refundable;
        Changeable = changeable;
        TicketingDeadline = ticketingDeadline;
        CancelPenalty = cancelPenalty;
        ChangePenalty = changePenalty;
        PartialRefundRequired = partialRefundRequired;
    }

    public FlightOfferSnapshotId FlightOfferSnapshotId { get; private set; }

    public bool Refundable { get; private set; }

    public bool Changeable { get; private set; }

    public Instant? TicketingDeadline { get; private set; }

    public MoneyValue? CancelPenalty { get; private set; }

    public MoneyValue? ChangePenalty { get; private set; }

    public bool PartialRefundRequired { get; private set; }

    public IReadOnlyList<FlightBaggageAllowanceSnapshot> Baggage => _baggage;

    internal void AddBaggage(FlightBaggageAllowanceSnapshot allowance)
    {
        ArgumentNullException.ThrowIfNull(allowance);
        _baggage.Add(allowance);
    }
}

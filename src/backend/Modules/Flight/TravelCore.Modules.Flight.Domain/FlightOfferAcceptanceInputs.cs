using NodaTime;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Flight.Domain;

public sealed record FlightFareRulesDraft(
    bool Refundable,
    bool Changeable,
    Instant? TicketingDeadline = null,
    MoneyValue? CancelPenalty = null,
    MoneyValue? ChangePenalty = null,
    bool PartialRefundRequired = false);

public sealed record FlightPassengerCategoryFareLine(
    FlightPassengerCategory Category,
    int PassengerCount,
    MoneyValue Amount);

public sealed record FlightBaggageAllowanceDraft(
    int? Quantity = null,
    decimal? Weight = null,
    string? Unit = null,
    string? Category = null,
    FlightPassengerCategory? PassengerCategory = null);

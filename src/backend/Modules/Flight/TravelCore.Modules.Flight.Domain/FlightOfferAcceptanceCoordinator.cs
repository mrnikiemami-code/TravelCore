using NodaTime;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Applies R4 commercial revalidation outcomes before immutable snapshot acceptance.
/// Timeout/Unknown and Changed are not accept authorities.
/// </summary>
public static class FlightOfferAcceptanceCoordinator
{
    public static FlightOfferSnapshot Accept(
        FlightBooking booking,
        Instant now,
        FlightOfferSourceResult offer,
        FlightOfferSnapshot? existingAccepted = null,
        MoneyValue? previouslyObservedTotal = null)
    {
        ArgumentNullException.ThrowIfNull(booking);
        ArgumentNullException.ThrowIfNull(offer);
        EnsureAcceptable(offer);

        return FlightOfferSnapshot.Accept(
            booking,
            now,
            offer.SourceKey.Value,
            offer.SourceOfferReference!,
            offer.QuotedAt!.Value,
            offer.OfferExpiresAt!.Value,
            offer.BaseFare!,
            offer.Taxes!,
            offer.Fees!,
            offer.TotalAmount!,
            offer.Segments!,
            offer.Passengers!,
            ToFareRules(offer.FareRules!),
            existingAccepted,
            previouslyObservedTotal,
            ToCategoryFares(offer.CategoryFares),
            ToBaggage(offer.Baggage),
            offer.Cabin,
            offer.BookingClass,
            offer.FareBasis,
            offer.FareFamily);
    }

    public static FlightOfferSourceResult MapCanceledToUnknown(FlightSourceKey sourceKey, Instant now) =>
        FlightOfferSourceResult.Unknown(sourceKey, now);

    public static void EnsureAcceptable(FlightOfferSourceResult offer)
    {
        ArgumentNullException.ThrowIfNull(offer);
        switch (offer.Outcome)
        {
            case FlightOfferOutcome.Available:
                return;
            case FlightOfferOutcome.Unknown:
                throw new InvalidOperationException("Unknown offer revalidation cannot be accepted.");
            case FlightOfferOutcome.Unavailable:
                throw new InvalidOperationException("Unavailable offer cannot be accepted.");
            case FlightOfferOutcome.Changed:
                throw new InvalidOperationException("Changed offer requires a requote.");
            default:
                throw new InvalidOperationException("Offer outcome cannot be accepted.");
        }
    }

    private static FlightFareRulesDraft ToFareRules(FlightFareRulesFact rules) =>
        new(
            rules.Refundable,
            rules.Changeable,
            rules.TicketingDeadline,
            rules.CancelPenalty,
            rules.ChangePenalty,
            rules.PartialRefundRequired);

    private static IReadOnlyList<FlightPassengerCategoryFareLine>? ToCategoryFares(
        IReadOnlyList<FlightPassengerCategoryFare>? fares) =>
        fares is null
            ? null
            : fares.Select(f => new FlightPassengerCategoryFareLine(f.Category, f.PassengerCount, f.Amount)).ToArray();

    private static IReadOnlyList<FlightBaggageAllowanceDraft>? ToBaggage(
        IReadOnlyList<FlightBaggageAllowanceFact>? baggage) =>
        baggage is null
            ? null
            : baggage.Select(b => new FlightBaggageAllowanceDraft(
                b.Quantity,
                b.Weight,
                b.Unit,
                b.Category,
                b.PassengerCategory)).ToArray();
}

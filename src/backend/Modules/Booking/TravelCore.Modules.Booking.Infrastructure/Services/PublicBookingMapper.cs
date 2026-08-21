using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using BookingAggregate = TravelCore.Modules.Booking.Domain.Booking;

namespace TravelCore.Modules.Booking.Infrastructure.Services;

internal static class PublicBookingMapper
{
    public static PublicBookingInitiationResponse ToInitiation(
        BookingAggregate booking,
        CapacityHold? hold,
        string? rawAccessToken)
    {
        return new PublicBookingInitiationResponse(
            booking.Id.Value,
            booking.Status.ToString(),
            booking.Source.Kind.ToString(),
            booking.TourDeparture.LogicalId,
            rawAccessToken,
            AccessTokenIssued: rawAccessToken is not null,
            Confirmed: booking.Status == BookingStatus.Confirmed,
            MapMonetary(booking.MonetarySnapshot),
            MapHold(hold),
            booking.Passengers.OrderBy(x => x.Sequence).Select(MapPassenger).ToList());
    }

    public static PublicBookingRead ToRead(BookingAggregate booking, CapacityHold? hold)
    {
        return new PublicBookingRead(
            booking.Id.Value,
            booking.Status.ToString(),
            booking.Source.Kind.ToString(),
            booking.TourDeparture.LogicalId,
            Confirmed: booking.Status == BookingStatus.Confirmed,
            booking.Contact is null
                ? null
                : new PublicBookingContactRead(
                    booking.Contact.DisplayName,
                    booking.Contact.Email,
                    booking.Contact.Phone),
            booking.Passengers.OrderBy(x => x.Sequence).Select(MapPassenger).ToList(),
            MapMonetary(booking.MonetarySnapshot),
            MapHold(hold));
    }

    private static PublicBookingPassengerRead MapPassenger(BookingPassenger passenger) =>
        new(passenger.Id.Value, passenger.GivenName, passenger.FamilyName, passenger.Category.ToString(), passenger.Sequence);

    private static PublicBookingHoldRead? MapHold(CapacityHold? hold) =>
        hold is null
            ? null
            : new PublicBookingHoldRead(hold.Status.ToString(), hold.ExpiresAt.ToDateTimeOffset(), hold.SeatCount);

    private static PublicBookingMonetaryRead? MapMonetary(BookingMonetarySnapshot? snapshot)
    {
        if (snapshot is null)
        {
            return null;
        }

        return new PublicBookingMonetaryRead(
            snapshot.QuoteReference.LogicalId,
            snapshot.SourcePriceId,
            snapshot.Total.Currency.Value,
            snapshot.Total.Amount,
            snapshot.QuoteExpiresAt.ToDateTimeOffset(),
            snapshot.Components
                .OrderBy(x => x.SortOrder)
                .Select(component => new PublicBookingMonetaryComponentRead(
                    component.Kind.ToString(),
                    new PublicBookingMoneyRead(component.Money.Amount, component.Money.Currency.Value),
                    component.SortOrder,
                    component.Code,
                    component.Label))
                .ToList());
    }
}

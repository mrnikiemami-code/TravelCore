using NodaTime;
using TravelCore.Modules.AgencyMarketplace.Contracts;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using BookingAggregate = TravelCore.Modules.Booking.Domain.Booking;

namespace TravelCore.Modules.Booking.Infrastructure.Services;

/// <summary>
/// Internal Booking creation boundary (TC-P19-T007 / P19-R7).
/// Direct creation needs no AgencyMarketplace dependency at call time.
/// Agency-originated creation validates logical refs through AgencyMarketplace.Contracts only.
/// Does not expose public API, Confirm, Payment, commission, or agency acceptance.
/// </summary>
public sealed class BookingCreationService
{
    private readonly BookingDbContext _db;
    private readonly IAgencyOriginContextQuery _origin;

    public BookingCreationService(BookingDbContext db, IAgencyOriginContextQuery origin)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(origin);
        _db = db;
        _origin = origin;
    }

    public async Task<BookingId> CreateDirectAsync(
        TourDepartureReference tourDeparture,
        Instant now,
        CancellationToken cancellationToken = default)
    {
        var booking = BookingAggregate.Create(tourDeparture, now, BookingSourceContext.Direct());
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync(cancellationToken);
        return booking.Id;
    }

    public async Task<BookingId> CreateAgencyAsync(
        TourDepartureReference tourDeparture,
        Instant now,
        Guid agencyProfileId,
        Guid? agencyOfferId,
        CancellationToken cancellationToken = default)
    {
        if (agencyProfileId == Guid.Empty)
        {
            throw new ArgumentException("AgencyProfileReference cannot be empty.", nameof(agencyProfileId));
        }

        var profile = await _origin.GetProfileAsync(agencyProfileId, cancellationToken)
            ?? throw new InvalidOperationException("AgencyProfile was not found.");

        AgencyOfferReference? offer = null;
        if (agencyOfferId is { } offerId)
        {
            if (offerId == Guid.Empty)
            {
                throw new ArgumentException("AgencyOfferReference cannot be empty.", nameof(agencyOfferId));
            }

            var offerFacts = await _origin.GetOfferAsync(offerId, cancellationToken)
                ?? throw new InvalidOperationException("AgencyOffer was not found.");

            if (offerFacts.AgencyProfileId != profile.AgencyProfileId)
            {
                throw new InvalidOperationException(
                    "AgencyOfferReference must belong to the stated AgencyProfileReference.");
            }

            if (offerFacts.ReferencedTourDepartureId is { } referencedDeparture
                && referencedDeparture != tourDeparture.LogicalId)
            {
                throw new InvalidOperationException(
                    "AgencyOffer referenced TourDeparture must match the Booking TourDeparture target.");
            }

            offer = new AgencyOfferReference(offerId);
        }

        var source = BookingSourceContext.ForAgency(new AgencyProfileReference(profile.AgencyProfileId), offer);
        var booking = BookingAggregate.Create(tourDeparture, now, source);
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync(cancellationToken);
        return booking.Id;
    }
}

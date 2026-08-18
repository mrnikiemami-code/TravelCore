using NodaTime;
using TravelCore.Modules.Booking.Contracts;

namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Transactional Tour Booking aggregate (TC-P19-T002 / P19-R2 / P19-R4 / P19-R5 / P19-R7).
/// Targets one logical TourDeparture. Owns transaction-time contact/passengers/monetary snapshot.
/// Direct and Agency-originated bookings share this aggregate.
/// </summary>
public sealed class Booking
{
    private readonly List<BookingPassenger> _passengers = [];

    private Booking()
    {
        Source = null!;
    }

    private Booking(
        BookingId id,
        TourDepartureReference tourDeparture,
        Instant createdAt,
        BookingSourceContext source)
    {
        ArgumentNullException.ThrowIfNull(source);
        Id = id;
        TourDeparture = tourDeparture;
        Status = BookingStatus.Pending;
        CreatedAt = createdAt;
        StatusChangedAt = createdAt;
        Source = source;
    }

    public BookingId Id { get; private set; }

    public TourDepartureReference TourDeparture { get; private set; }

    public BookingStatus Status { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant StatusChangedAt { get; private set; }

    public BookingSourceContext Source { get; private set; }

    public BookingContactSnapshot? Contact { get; private set; }

    public BookingActorReference? ActorReference { get; private set; }

    public BookingPartyReference? PartyReference { get; private set; }

    public BookingMonetarySnapshot? MonetarySnapshot { get; private set; }

    public IReadOnlyList<BookingPassenger> Passengers => _passengers;

    public static Booking Create(TourDepartureReference tourDeparture, Instant now) =>
        Create(tourDeparture, now, BookingSourceContext.Direct());

    public static Booking Create(
        TourDepartureReference tourDeparture,
        Instant now,
        BookingSourceContext source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (tourDeparture.LogicalId == Guid.Empty)
        {
            throw new ArgumentException("TourDeparture reference cannot be empty.", nameof(tourDeparture));
        }

        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        return new Booking(BookingId.New(), tourDeparture, now, source);
    }

    public void CancelPending(Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("StatusChangedAt cannot be default.", nameof(now));
        }

        if (Status == BookingStatus.Cancelled)
        {
            return;
        }

        if (Status != BookingStatus.Pending)
        {
            throw new InvalidOperationException(
                "Confirmed Booking cancellation is deferred (P19-R6). Only Pending Booking can be cancelled.");
        }

        Status = BookingStatus.Cancelled;
        StatusChangedAt = now;
    }

    public void SetContact(BookingContactSnapshot contact)
    {
        ArgumentNullException.ThrowIfNull(contact);
        EnsurePendingPeopleEdit();
        Contact = contact;
    }

    public void SetActorReference(BookingActorReference? actor)
    {
        EnsurePendingPeopleEdit();
        if (actor is { ActorId: var id } && id == Guid.Empty)
        {
            throw new ArgumentException("BookingActorReference cannot be empty.", nameof(actor));
        }

        ActorReference = actor;
    }

    public void SetPartyReference(BookingPartyReference? party)
    {
        EnsurePendingPeopleEdit();
        if (party is { PartyId: var id } && id == Guid.Empty)
        {
            throw new ArgumentException("BookingPartyReference cannot be empty.", nameof(party));
        }

        PartyReference = party;
    }

    public BookingPassenger AddPassenger(
        string givenName,
        string familyName,
        TravelerCategory category,
        int? activeHeldSeatCount)
    {
        EnsurePendingPeopleEdit();
        EnsurePassengerCountFits(PassengerCount + 1, activeHeldSeatCount);
        var nextSequence = _passengers.Count == 0 ? 0 : _passengers.Max(x => x.Sequence) + 1;
        var passenger = BookingPassenger.Create(givenName, familyName, category, nextSequence);
        _passengers.Add(passenger);
        return passenger;
    }

    public void UpdatePassenger(
        BookingPassengerId passengerId,
        string givenName,
        string familyName,
        TravelerCategory category)
    {
        EnsurePendingPeopleEdit();
        var passenger = _passengers.SingleOrDefault(x => x.Id == passengerId)
            ?? throw new InvalidOperationException("BookingPassenger was not found on this Booking.");
        passenger.Rename(givenName, familyName);
        passenger.Recategorize(category);
    }

    public void RemovePassenger(BookingPassengerId passengerId)
    {
        EnsurePendingPeopleEdit();
        var removed = _passengers.RemoveAll(x => x.Id == passengerId);
        if (removed == 0)
        {
            throw new InvalidOperationException("BookingPassenger was not found on this Booking.");
        }
    }

    public void AcceptQuote(AuthoritativeQuoteFacts quote, Instant now)
    {
        ArgumentNullException.ThrowIfNull(quote);
        if (now == default)
        {
            throw new ArgumentException("Acceptance time cannot be default.", nameof(now));
        }

        if (Status != BookingStatus.Pending)
        {
            throw new InvalidOperationException("Quote can be accepted only while Booking is Pending.");
        }

        if (quote.IsExpired(now))
        {
            throw new InvalidOperationException("Expired Quote cannot be newly accepted as BookingMonetarySnapshot.");
        }

        EnsureQuoteTargetsThisDeparture(quote);

        if (MonetarySnapshot is not null)
        {
            if (MonetarySnapshot.QuoteReference == quote.QuoteReference)
            {
                return;
            }

            throw new InvalidOperationException(
                "Booking already has an accepted monetary snapshot; attaching a different Quote is not implemented in T005.");
        }

        MonetarySnapshot = BookingMonetarySnapshot.CopyFrom(Id, quote, now);
    }

    public int PassengerCount => _passengers.Count;

    private void EnsureQuoteTargetsThisDeparture(AuthoritativeQuoteFacts quote)
    {
        if (!string.Equals(quote.TargetType, BookingOwnershipBoundary.InitialTarget, StringComparison.Ordinal)
            || quote.TargetId != TourDeparture.LogicalId)
        {
            throw new InvalidOperationException(
                "Quote target must match this Booking TourDeparture. Arbitrary Quote reuse is forbidden.");
        }
    }

    private void EnsurePendingPeopleEdit()
    {
        if (Status != BookingStatus.Pending)
        {
            throw new InvalidOperationException("Passenger/contact facts can be edited only while Booking is Pending.");
        }
    }

    private static void EnsurePassengerCountFits(int nextCount, int? activeHeldSeatCount)
    {
        if (activeHeldSeatCount is { } held && nextCount > held)
        {
            throw new InvalidOperationException(
                "PassengerCount cannot exceed Active CapacityHold SeatCount. Resize/re-hold is not implemented in T004.");
        }
    }
}

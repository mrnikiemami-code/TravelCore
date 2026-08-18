using NodaTime;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// Hotel stay transaction aggregate. PayNow confirmation requires Payment + supplier evidence (P21-R6).
/// </summary>
public sealed class HotelBooking
{
    private readonly List<RoomReservation> _rooms = [];

    private HotelBooking()
    {
        Place = default;
        Contact = null!;
        Status = HotelBookingStatus.Pending;
        Version = 0;
    }

    private HotelBooking(
        HotelBookingId id,
        HotelPlaceReference place,
        LocalDate checkInDate,
        LocalDate checkOutDate,
        HotelBookingContactSnapshot contact)
    {
        Id = id;
        Place = place;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        Contact = contact;
        Status = HotelBookingStatus.Pending;
        Version = 0;
    }

    public HotelBookingId Id { get; private set; }

    public HotelBookingStatus Status { get; private set; }

    public Instant? ConfirmedAt { get; private set; }

    public Instant? CancelledAt { get; private set; }

    public long Version { get; private set; }

    public HotelPlaceReference Place { get; private set; }

    public LocalDate CheckInDate { get; private set; }

    public LocalDate CheckOutDate { get; private set; }

    public int Nights => Period.Between(CheckInDate, CheckOutDate, PeriodUnits.Days).Days;

    public HotelBookingContactSnapshot Contact { get; private set; }

    /// <summary>
    /// Optional logical authenticated actor id captured at initiation.
    /// Not an Identity/Party entity and not an authorization credential by itself.
    /// </summary>
    public Guid? ActorAccountId { get; private set; }

    public IReadOnlyList<RoomReservation> Rooms => _rooms;

    public int RoomCount => _rooms.Count;

    public IEnumerable<HotelBookingGuest> Guests => _rooms.SelectMany(r => r.Guests);

    public int GuestCount => _rooms.Sum(r => r.GuestCount);

    public int AdultCount => _rooms.Sum(r => r.AdultCount);

    public int ChildCount => _rooms.Sum(r => r.ChildCount);

    public HotelBookingGuest LeadGuest =>
        Guests.Single(g => g.IsLeadGuest);

    public static HotelBooking Create(
        HotelPlaceReference place,
        LocalDate checkInDate,
        LocalDate checkOutDate,
        HotelBookingContactSnapshot contact,
        IReadOnlyList<RoomReservationSpecification> rooms)
    {
        ArgumentNullException.ThrowIfNull(contact);
        ArgumentNullException.ThrowIfNull(rooms);

        if (checkOutDate <= checkInDate)
        {
            throw new ArgumentException(
                "CheckOutDate must be later than CheckInDate.",
                nameof(checkOutDate));
        }

        if (rooms.Count == 0)
        {
            throw new ArgumentException("HotelBooking requires at least one RoomReservation.", nameof(rooms));
        }

        var booking = new HotelBooking(
            HotelBookingId.New(),
            place,
            checkInDate,
            checkOutDate,
            contact);

        var ordinal = 1;
        foreach (var roomSpec in rooms)
        {
            ArgumentNullException.ThrowIfNull(roomSpec);
            booking._rooms.Add(RoomReservation.Create(booking.Id, ordinal, roomSpec.Guests));
            ordinal++;
        }

        var leadCount = booking.Guests.Count(g => g.IsLeadGuest);
        if (leadCount != 1)
        {
            throw new ArgumentException(
                "HotelBooking requires exactly one LeadGuest.",
                nameof(rooms));
        }

        return booking;
    }

    public void AttachActorAccount(Guid actorAccountId)
    {
        if (actorAccountId == Guid.Empty)
        {
            throw new ArgumentException("Actor account id cannot be empty.", nameof(actorAccountId));
        }

        ActorAccountId = actorAccountId;
    }

    /// <summary>
    /// Constrained Pending → Confirmed. PayNow requires authoritative Payment evidence.
    /// Already-Confirmed rows stay Confirmed (T005 history is not downgraded).
    /// Not a generic Confirm/SetConfirmed surface.
    /// </summary>
    public void ConfirmFromAuthoritativeSupplierReservation(
        HotelSupplierReservation reservation,
        Instant now,
        HotelPlaceReference reportedPlace,
        LocalDate reportedCheckIn,
        LocalDate reportedCheckOut,
        IReadOnlyCollection<RoomReservationId> confirmedRooms,
        MoneyValue? reportedTotal,
        bool? cancellationTermsMatch,
        HotelBookingMonetarySnapshot monetary,
        IReadOnlyList<HotelBookingReconciliationIssue> existingIssues,
        HotelBookingPaymentEvidence? paymentEvidence = null)
    {
        ArgumentNullException.ThrowIfNull(reservation);
        ArgumentNullException.ThrowIfNull(confirmedRooms);
        ArgumentNullException.ThrowIfNull(monetary);
        ArgumentNullException.ThrowIfNull(existingIssues);
        EnsureClock(now);

        if (Status == HotelBookingStatus.Confirmed)
        {
            if (reservation.HotelBookingId.Equals(Id)
                && reservation.Status == HotelSupplierReservationStatus.Confirmed)
            {
                return;
            }

            throw new InvalidOperationException(
                "Confirmed HotelBooking cannot be reopened or reassigned from later evidence.");
        }

        if (Status == HotelBookingStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Cancelled HotelBooking cannot be reopened by confirmation evidence.");
        }

        if (Status != HotelBookingStatus.Pending)
        {
            throw new InvalidOperationException($"HotelBooking in status {Status} cannot become Confirmed.");
        }

        if (!reservation.HotelBookingId.Equals(Id))
        {
            throw new InvalidOperationException(
                "Supplier reservation evidence for another HotelBooking cannot confirm this booking.");
        }

        if (reservation.Status != HotelSupplierReservationStatus.Confirmed)
        {
            throw new InvalidOperationException(
                "HotelBooking confirmation requires an authoritative Confirmed HotelSupplierReservation.");
        }

        if (existingIssues.Any(issue => issue.BlocksConfirmation))
        {
            throw new InvalidOperationException(
                "Blocking HotelBookingReconciliationIssue prevents confirmation.");
        }

        var issues = CollectConfirmationIssues(
            reservation,
            reportedPlace,
            reportedCheckIn,
            reportedCheckOut,
            confirmedRooms,
            reportedTotal,
            cancellationTermsMatch,
            monetary);
        if (issues.Count > 0)
        {
            throw new InvalidOperationException(
                "Authoritative reservation evidence does not satisfy HotelBooking confirmation invariants: "
                + string.Join(", ", issues));
        }

        EnsurePayNowPaymentEvidence(paymentEvidence, monetary);
        ApplyConfirmed(now);
    }

    /// <summary>
    /// Constrained Pending → Confirmed when caller has already verified stay/money/room evidence.
    /// PayNow still requires Payment evidence unless the row is already Confirmed.
    /// </summary>
    public void ConfirmFromAuthoritativeSupplierReservation(
        HotelSupplierReservation reservation,
        IReadOnlyCollection<HotelBookingReconciliationIssue> openIssues,
        Instant now,
        HotelBookingPaymentEvidence? paymentEvidence = null)
    {
        ArgumentNullException.ThrowIfNull(reservation);
        ArgumentNullException.ThrowIfNull(openIssues);
        EnsureClock(now);

        if (Status == HotelBookingStatus.Confirmed)
        {
            if (reservation.HotelBookingId.Equals(Id)
                && reservation.Status == HotelSupplierReservationStatus.Confirmed)
            {
                return;
            }

            throw new InvalidOperationException(
                "Confirmed HotelBooking cannot be reopened or reassigned from later evidence.");
        }

        if (Status == HotelBookingStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Cancelled HotelBooking cannot be reopened by confirmation evidence.");
        }

        if (Status != HotelBookingStatus.Pending)
        {
            throw new InvalidOperationException($"HotelBooking in status {Status} cannot become Confirmed.");
        }

        if (!reservation.HotelBookingId.Equals(Id))
        {
            throw new InvalidOperationException(
                "Supplier reservation evidence for another HotelBooking cannot confirm this booking.");
        }

        if (reservation.Status != HotelSupplierReservationStatus.Confirmed)
        {
            throw new InvalidOperationException(
                "HotelBooking confirmation requires an authoritative Confirmed HotelSupplierReservation.");
        }

        if (openIssues.Any(issue => issue.BlocksConfirmation))
        {
            throw new InvalidOperationException(
                "Blocking HotelBookingReconciliationIssue prevents confirmation.");
        }

        if (paymentEvidence is null)
        {
            throw new InvalidOperationException(
                "PayNow HotelBooking confirmation requires authoritative Payment success evidence.");
        }

        ApplyConfirmed(now);
    }

    /// <summary>
    /// Dual-evidence PayNow confirmation: Payment Succeeded AND SupplierReservation Confirmed.
    /// </summary>
    public void ConfirmFromAuthoritativePaymentAndSupplierEvidence(
        HotelSupplierReservation reservation,
        HotelBookingPaymentEvidence paymentEvidence,
        Instant now,
        HotelPlaceReference reportedPlace,
        LocalDate reportedCheckIn,
        LocalDate reportedCheckOut,
        IReadOnlyCollection<RoomReservationId> confirmedRooms,
        MoneyValue? reportedTotal,
        bool? cancellationTermsMatch,
        HotelBookingMonetarySnapshot monetary,
        IReadOnlyList<HotelBookingReconciliationIssue> existingIssues)
    {
        ArgumentNullException.ThrowIfNull(paymentEvidence);
        ConfirmFromAuthoritativeSupplierReservation(
            reservation,
            now,
            reportedPlace,
            reportedCheckIn,
            reportedCheckOut,
            confirmedRooms,
            reportedTotal,
            cancellationTermsMatch,
            monetary,
            existingIssues,
            paymentEvidence);
    }

    /// <summary>
    /// System compensation terminalization: Pending → Cancelled after authoritative full Refund.
    /// Does not cancel Confirmed HotelBooking (R7). Not a generic Cancel/SetCancelled surface.
    /// </summary>
    public void CancelFromAuthoritativePaymentCompensation(Instant now)
    {
        EnsureClock(now);
        if (Status == HotelBookingStatus.Cancelled)
        {
            return;
        }

        if (Status == HotelBookingStatus.Confirmed)
        {
            throw new InvalidOperationException(
                "Confirmed HotelBooking cannot be cancelled by payment compensation.");
        }

        if (Status != HotelBookingStatus.Pending)
        {
            throw new InvalidOperationException($"HotelBooking in status {Status} cannot become Cancelled.");
        }

        Status = HotelBookingStatus.Cancelled;
        CancelledAt = now;
        IncrementVersion();
    }

    /// <summary>
    /// Constrained Confirmed → Cancelled after authoritative supplier cancellation (P21-R7).
    /// Not a generic Cancel/SetCancelled/ForceCancel surface. R6 compensation remains Pending-only.
    /// </summary>
    public void CancelFromAuthoritativeSupplierCancellation(
        HotelSupplierReservation reservation,
        Instant now)
    {
        ArgumentNullException.ThrowIfNull(reservation);
        EnsureClock(now);

        if (Status == HotelBookingStatus.Cancelled)
        {
            if (reservation.HotelBookingId.Equals(Id)
                && reservation.Status == HotelSupplierReservationStatus.Cancelled)
            {
                return;
            }

            throw new InvalidOperationException(
                "Cancelled HotelBooking cannot be reopened or reassigned from later cancellation evidence.");
        }

        if (Status != HotelBookingStatus.Confirmed)
        {
            throw new InvalidOperationException(
                "R7 customer cancellation requires HotelBookingStatus Confirmed.");
        }

        if (!reservation.HotelBookingId.Equals(Id))
        {
            throw new InvalidOperationException(
                "Supplier cancellation evidence for another HotelBooking cannot cancel this booking.");
        }

        if (reservation.Status != HotelSupplierReservationStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "HotelBooking cancellation requires an authoritatively Cancelled HotelSupplierReservation.");
        }

        Status = HotelBookingStatus.Cancelled;
        CancelledAt = now;
        IncrementVersion();
    }

    private void EnsurePayNowPaymentEvidence(
        HotelBookingPaymentEvidence? paymentEvidence,
        HotelBookingMonetarySnapshot monetary)
    {
        if (paymentEvidence is null)
        {
            throw new InvalidOperationException(
                "PayNow HotelBooking confirmation requires authoritative Payment success evidence.");
        }

        if (!paymentEvidence.HotelBookingId.Equals(Id))
        {
            throw new InvalidOperationException(
                "Payment evidence for another HotelBooking cannot confirm this booking.");
        }

        if (!paymentEvidence.MatchesMonetarySnapshot(monetary))
        {
            throw new InvalidOperationException(
                "Payment evidence amount/currency does not match HotelBookingMonetarySnapshot.");
        }
    }

    private void ApplyConfirmed(Instant now)
    {
        Status = HotelBookingStatus.Confirmed;
        ConfirmedAt = now;
        IncrementVersion();
    }

    private void IncrementVersion() => Version++;

    public IReadOnlyList<HotelBookingReconciliationIssueKind> CollectConfirmationIssues(
        HotelSupplierReservation reservation,
        HotelPlaceReference reportedPlace,
        LocalDate reportedCheckIn,
        LocalDate reportedCheckOut,
        IReadOnlyCollection<RoomReservationId> confirmedRooms,
        MoneyValue? reportedTotal,
        bool? cancellationTermsMatch,
        HotelBookingMonetarySnapshot monetary)
    {
        ArgumentNullException.ThrowIfNull(reservation);
        ArgumentNullException.ThrowIfNull(confirmedRooms);
        ArgumentNullException.ThrowIfNull(monetary);

        var issues = new List<HotelBookingReconciliationIssueKind>();
        if (reportedPlace.PlaceId != Place.PlaceId)
        {
            issues.Add(HotelBookingReconciliationIssueKind.HotelMismatch);
        }

        if (reportedCheckIn != CheckInDate || reportedCheckOut != CheckOutDate)
        {
            issues.Add(HotelBookingReconciliationIssueKind.StayMismatch);
        }

        var expectedRooms = _rooms.Select(r => r.Id).ToHashSet();
        var actualRooms = confirmedRooms.ToHashSet();
        if (actualRooms.Count != expectedRooms.Count || !expectedRooms.SetEquals(actualRooms))
        {
            issues.Add(HotelBookingReconciliationIssueKind.RoomSetMismatch);
        }

        if (reportedTotal is not null)
        {
            if (reportedTotal.Currency != monetary.CurrencyCode)
            {
                issues.Add(HotelBookingReconciliationIssueKind.CurrencyMismatch);
            }

            if (reportedTotal.Amount != monetary.Total.Amount)
            {
                issues.Add(HotelBookingReconciliationIssueKind.MonetaryMismatch);
            }
        }

        if (cancellationTermsMatch == false)
        {
            issues.Add(HotelBookingReconciliationIssueKind.CancellationTermsMismatch);
        }

        return issues;
    }

    private static void EnsureClock(Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("Instant cannot be default.", nameof(now));
        }
    }

    public void EnsureMatchesRateOffer(
        HotelPlaceReference place,
        LocalDate checkInDate,
        LocalDate checkOutDate,
        IEnumerable<RoomReservationId> roomIds)
    {
        ArgumentNullException.ThrowIfNull(roomIds);
        if (place.PlaceId != Place.PlaceId)
        {
            throw new ArgumentException("HotelPlaceReference does not match HotelBooking.", nameof(place));
        }

        if (checkInDate != CheckInDate || checkOutDate != CheckOutDate)
        {
            throw new ArgumentException("Stay dates do not match HotelBooking.");
        }

        var expected = _rooms.Select(r => r.Id).ToHashSet();
        var actual = roomIds.ToHashSet();
        if (actual.Count != expected.Count || !expected.SetEquals(actual))
        {
            throw new ArgumentException("Room set does not match HotelBooking.");
        }
    }

    /// <summary>
    /// Stay place/dates/rooms/occupancy cannot change after an accepted commercial snapshot.
    /// Amendment/requote workflow is deferred (P21-R5–R8).
    /// </summary>
    public void GuardAgainstSilentStayAmendmentAfterAcceptedRateOffer()
    {
        throw new InvalidOperationException(
            "HotelBooking place, stay dates, rooms, and occupancy cannot change after an accepted rate offer; requote/amendment is not implemented.");
    }
}

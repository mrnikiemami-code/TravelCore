using NodaTime;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// Immutable HotelBooking-owned transaction-time commercial offer (TC-P21-T004 / P21-R4).
/// Not live supplier rate, Payment, availability authority, or a named supplier reservation.
/// </summary>
public sealed class HotelRateOfferSnapshot
{
    public const int SourceKeyMaxLength = 64;
    public const int SourceOfferReferenceMaxLength = 128;

    private readonly List<HotelRoomRateSnapshot> _rooms = [];

    private HotelRateOfferSnapshot()
    {
        SourceKey = string.Empty;
        SourceOfferReference = string.Empty;
        Place = default;
        Monetary = null!;
        CancellationPolicy = null!;
    }

    private HotelRateOfferSnapshot(
        HotelRateOfferSnapshotId id,
        HotelBookingId hotelBookingId,
        HotelPlaceReference place,
        LocalDate checkInDate,
        LocalDate checkOutDate,
        string sourceKey,
        string sourceOfferReference,
        Instant quotedAt,
        Instant? offerExpiresAt,
        Instant acceptedAt)
    {
        Id = id;
        HotelBookingId = hotelBookingId;
        Place = place;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        SourceKey = sourceKey;
        SourceOfferReference = sourceOfferReference;
        QuotedAt = quotedAt;
        OfferExpiresAt = offerExpiresAt;
        AcceptedAt = acceptedAt;
        Monetary = null!;
        CancellationPolicy = null!;
    }

    public HotelRateOfferSnapshotId Id { get; private set; }

    public HotelBookingId HotelBookingId { get; private set; }

    public HotelPlaceReference Place { get; private set; }

    public LocalDate CheckInDate { get; private set; }

    public LocalDate CheckOutDate { get; private set; }

    public string SourceKey { get; private set; }

    public string SourceOfferReference { get; private set; }

    public Instant QuotedAt { get; private set; }

    public Instant? OfferExpiresAt { get; private set; }

    public Instant AcceptedAt { get; private set; }

    public HotelBookingMonetarySnapshot Monetary { get; private set; }

    public HotelCancellationPolicySnapshot CancellationPolicy { get; private set; }

    public IReadOnlyList<HotelRoomRateSnapshot> Rooms => _rooms;

    public bool IsSameOfferIdentity(string sourceKey, string sourceOfferReference)
    {
        var key = NormalizeRequired(sourceKey, SourceKeyMaxLength, nameof(sourceKey)).ToLowerInvariant();
        var offerRef = NormalizeRequired(sourceOfferReference, SourceOfferReferenceMaxLength, nameof(sourceOfferReference));
        return SourceKey == key && SourceOfferReference == offerRef;
    }

    /// <summary>
    /// Accepts an authoritative source offer as the immutable transaction snapshot.
    /// Same offer identity is idempotent; a different offer after acceptance conflicts.
    /// </summary>
    public static HotelRateOfferSnapshot Accept(
        HotelBooking booking,
        Instant now,
        HotelPlaceReference place,
        LocalDate checkInDate,
        LocalDate checkOutDate,
        string sourceKey,
        string sourceOfferReference,
        Instant quotedAt,
        Instant? offerExpiresAt,
        MoneyValue total,
        IReadOnlyList<HotelRoomRateLine> rooms,
        IReadOnlyList<HotelCancellationPenaltyRuleDraft> penaltyRules,
        HotelRateOfferSnapshot? existingAccepted = null,
        MoneyValue? payableNow = null,
        MoneyValue? payableAtProperty = null,
        IReadOnlyList<HotelChargeComponentLine>? charges = null,
        string? propertyTimeZoneId = null,
        string? publicExplanation = null)
    {
        ArgumentNullException.ThrowIfNull(booking);
        ArgumentNullException.ThrowIfNull(total);
        ArgumentNullException.ThrowIfNull(rooms);
        ArgumentNullException.ThrowIfNull(penaltyRules);
        EnsureClock(now, nameof(now));
        EnsureClock(quotedAt, nameof(quotedAt));

        var normalizedKey = NormalizeRequired(sourceKey, SourceKeyMaxLength, nameof(sourceKey)).ToLowerInvariant();
        var normalizedOfferRef = NormalizeRequired(
            sourceOfferReference,
            SourceOfferReferenceMaxLength,
            nameof(sourceOfferReference));

        if (existingAccepted is not null)
        {
            if (existingAccepted.IsSameOfferIdentity(normalizedKey, normalizedOfferRef))
            {
                return existingAccepted;
            }

            throw new InvalidOperationException(
                "A different rate offer is already accepted; requote is required.");
        }

        booking.EnsureMatchesRateOffer(place, checkInDate, checkOutDate, rooms.Select(r => r.RoomReservationId));

        if (offerExpiresAt is { } expires)
        {
            EnsureClock(expires, nameof(offerExpiresAt));
            if (expires <= now)
            {
                throw new InvalidOperationException("Expired rate offer cannot be accepted.");
            }
        }

        RejectToman(total.Currency);
        EnsureSameCurrency(total, payableNow, nameof(payableNow));
        EnsureSameCurrency(total, payableAtProperty, nameof(payableAtProperty));

        var snapshotId = HotelRateOfferSnapshotId.New();
        if (normalizedOfferRef.Equals(snapshotId.ToString(), StringComparison.OrdinalIgnoreCase)
            || Guid.TryParse(normalizedOfferRef, out var asGuid) && asGuid == snapshotId.Value)
        {
            throw new ArgumentException(
                "SourceOfferReference must not equal HotelRateOfferSnapshotId.",
                nameof(sourceOfferReference));
        }

        ValidateRoomCoverage(booking, rooms, total);
        var snapshot = new HotelRateOfferSnapshot(
            snapshotId,
            booking.Id,
            booking.Place,
            booking.CheckInDate,
            booking.CheckOutDate,
            normalizedKey,
            normalizedOfferRef,
            quotedAt,
            offerExpiresAt,
            now);

        foreach (var line in rooms)
        {
            snapshot._rooms.Add(new HotelRoomRateSnapshot(
                snapshot.Id,
                line.RoomReservationId,
                CopyMoney(line.Amount),
                NormalizeOptional(line.AvailabilitySelectionReference, HotelRoomRateSnapshot.ReferenceMaxLength),
                NormalizeOptional(line.SourceRateReference, HotelRoomRateSnapshot.ReferenceMaxLength),
                NormalizeOptional(line.BoardBasisCode, HotelRoomRateSnapshot.BoardBasisMaxLength)));
        }

        snapshot.Monetary = new HotelBookingMonetarySnapshot(
            snapshot.Id,
            booking.Id,
            CopyMoney(total)!,
            CopyMoney(payableNow),
            CopyMoney(payableAtProperty));

        if (charges is { Count: > 0 })
        {
            var ordinal = 1;
            foreach (var charge in charges)
            {
                ArgumentNullException.ThrowIfNull(charge);
                ArgumentNullException.ThrowIfNull(charge.Amount);
                var code = NormalizeRequired(charge.Code, HotelChargeComponentSnapshot.CodeMaxLength, nameof(charge.Code));
                EnsureSameCurrency(total, charge.Amount, nameof(charge.Amount));
                snapshot.Monetary.AddCharge(new HotelChargeComponentSnapshot(
                    snapshot.Id,
                    ordinal,
                    code,
                    CopyMoney(charge.Amount)!));
                ordinal++;
            }
        }

        snapshot.CancellationPolicy = BuildCancellationPolicy(
            snapshot.Id,
            total,
            penaltyRules,
            propertyTimeZoneId,
            publicExplanation);

        return snapshot;
    }

    private static void ValidateRoomCoverage(
        HotelBooking booking,
        IReadOnlyList<HotelRoomRateLine> rooms,
        MoneyValue total)
    {
        if (rooms.Count == 0)
        {
            throw new ArgumentException("Accepted offer must cover at least one RoomReservation.", nameof(rooms));
        }

        var bookingRoomIds = booking.Rooms.Select(r => r.Id).ToArray();
        var lineIds = rooms.Select(r => r.RoomReservationId).ToArray();
        if (lineIds.Distinct().Count() != lineIds.Length)
        {
            throw new ArgumentException("Duplicate RoomReservationId in room rate lines.", nameof(rooms));
        }

        if (lineIds.Length != bookingRoomIds.Length
            || bookingRoomIds.Any(id => !lineIds.Contains(id)))
        {
            throw new ArgumentException(
                "Accepted offer must cover every RoomReservation exactly once.",
                nameof(rooms));
        }

        var amounts = rooms.Select(r => r.Amount).ToArray();
        var withAmount = amounts.Count(a => a is not null);
        if (withAmount != 0 && withAmount != rooms.Count)
        {
            throw new ArgumentException(
                "Room amounts must be complete when any room amount is supplied.",
                nameof(rooms));
        }

        if (withAmount == rooms.Count)
        {
            CurrencyCode? roomCurrency = null;
            MoneyValue? sum = null;
            foreach (var amount in amounts)
            {
                ArgumentNullException.ThrowIfNull(amount);
                RejectToman(amount.Currency);
                if (roomCurrency is null)
                {
                    roomCurrency = amount.Currency;
                    sum = amount;
                }
                else if (roomCurrency != amount.Currency)
                {
                    throw new InvalidOperationException(
                        "Mixed room currencies cannot be accepted; implicit FX is forbidden.");
                }
                else
                {
                    sum = sum!.Add(amount);
                }
            }

            if (roomCurrency != total.Currency || sum!.Amount != total.Amount)
            {
                throw new ArgumentException(
                    "Booking total must equal the sum of room amounts when room amounts are supplied.",
                    nameof(total));
            }
        }
    }

    private static HotelCancellationPolicySnapshot BuildCancellationPolicy(
        HotelRateOfferSnapshotId snapshotId,
        MoneyValue total,
        IReadOnlyList<HotelCancellationPenaltyRuleDraft> penaltyRules,
        string? propertyTimeZoneId,
        string? publicExplanation)
    {
        if (penaltyRules.Count == 0)
        {
            throw new ArgumentException("Cancellation policy requires at least one penalty rule.", nameof(penaltyRules));
        }

        var timeZone = NormalizeOptional(propertyTimeZoneId, HotelCancellationPolicySnapshot.TimeZoneIdMaxLength);
        if (timeZone is not null && DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZone) is null)
        {
            throw new ArgumentException(
                "PropertyTimeZoneId must be a valid IANA timezone identifier.",
                nameof(propertyTimeZoneId));
        }

        var ordered = penaltyRules
            .Select((rule, index) => (rule, index))
            .OrderBy(x => x.rule.EffectiveFrom)
            .ThenBy(x => x.index)
            .Select(x => x.rule)
            .ToArray();

        for (var i = 0; i < ordered.Length; i++)
        {
            var rule = ordered[i];
            ArgumentNullException.ThrowIfNull(rule.Penalty);
            EnsureClock(rule.EffectiveFrom, nameof(rule.EffectiveFrom));
            if (rule.EffectiveUntil is { } until)
            {
                EnsureClock(until, nameof(rule.EffectiveUntil));
                if (until <= rule.EffectiveFrom)
                {
                    throw new ArgumentException("Penalty EffectiveUntil must be later than EffectiveFrom.");
                }
            }

            EnsureSameCurrency(total, rule.Penalty, nameof(rule.Penalty));
            if (rule.Penalty.Amount < 0m)
            {
                throw new ArgumentException("PenaltyAmount must be >= 0.");
            }

            if (rule.Penalty.Amount > total.Amount)
            {
                throw new ArgumentException("PenaltyAmount must be <= TotalAmount.");
            }

            if (i > 0)
            {
                var previous = ordered[i - 1];
                var previousUntil = previous.EffectiveUntil ?? Instant.MaxValue;
                if (rule.EffectiveFrom < previousUntil)
                {
                    throw new ArgumentException("Overlapping cancellation penalty intervals are rejected.");
                }
            }
        }

        var policy = new HotelCancellationPolicySnapshot(
            snapshotId,
            timeZone,
            NormalizeOptional(publicExplanation, HotelCancellationPolicySnapshot.ExplanationMaxLength));

        var ordinal = 1;
        foreach (var rule in ordered)
        {
            policy.AddRule(new HotelCancellationPenaltyRule(
                snapshotId,
                ordinal,
                rule.EffectiveFrom,
                rule.EffectiveUntil,
                CopyMoney(rule.Penalty)!));
            ordinal++;
        }

        return policy;
    }

    private static void RejectToman(CurrencyCode currency)
    {
        if (currency.Value.Equals("TOMAN", StringComparison.Ordinal))
        {
            throw new ArgumentException("Toman is not a CurrencyCode for HotelBooking monetary snapshots.");
        }
    }

    private static void EnsureSameCurrency(MoneyValue total, MoneyValue? other, string paramName)
    {
        if (other is not null && other.Currency != total.Currency)
        {
            throw new ArgumentException(
                "Monetary values on one accepted offer must share a single CurrencyCode.",
                paramName);
        }
    }

    private static MoneyValue? CopyMoney(MoneyValue? money) =>
        money is null ? null : new MoneyValue(money.Amount, money.Currency);

    private static string NormalizeRequired(string value, int maxLength, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", paramName);
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"Max length is {maxLength}.", paramName);
        }

        return trimmed;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"Max length is {maxLength}.");
        }

        return trimmed;
    }

    private static void EnsureClock(Instant instant, string paramName)
    {
        if (instant == default)
        {
            throw new ArgumentException("Instant cannot be default.", paramName);
        }
    }
}

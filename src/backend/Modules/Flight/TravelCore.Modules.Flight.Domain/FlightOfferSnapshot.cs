using NodaTime;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Immutable FlightBooking-owned transaction-time commercial offer (TC-P22-T004 / P22-R4).
/// Not live search, Payment, availability authority, PNR, or a named supplier reservation.
/// </summary>
public sealed class FlightOfferSnapshot
{
    public const int SourceKeyMaxLength = 64;
    public const int SourceOfferReferenceMaxLength = 128;
    public const int CabinMaxLength = 32;
    public const int BookingClassMaxLength = 8;
    public const int FareBasisMaxLength = 16;
    public const int FareFamilyMaxLength = 64;

    private FlightOfferSnapshot()
    {
        SourceKey = string.Empty;
        SourceOfferReference = string.Empty;
        Monetary = null!;
        FareRules = null!;
    }

    private FlightOfferSnapshot(
        FlightOfferSnapshotId id,
        FlightBookingId flightBookingId,
        FlightTripType tripType,
        string sourceKey,
        string sourceOfferReference,
        Instant quotedAt,
        Instant offerExpiresAt,
        Instant acceptedAt,
        string? cabin,
        string? bookingClass,
        string? fareBasis,
        string? fareFamily)
    {
        Id = id;
        FlightBookingId = flightBookingId;
        TripType = tripType;
        SourceKey = sourceKey;
        SourceOfferReference = sourceOfferReference;
        QuotedAt = quotedAt;
        OfferExpiresAt = offerExpiresAt;
        AcceptedAt = acceptedAt;
        Cabin = cabin;
        BookingClass = bookingClass;
        FareBasis = fareBasis;
        FareFamily = fareFamily;
        Monetary = null!;
        FareRules = null!;
    }

    public FlightOfferSnapshotId Id { get; private set; }

    public FlightBookingId FlightBookingId { get; private set; }

    public FlightTripType TripType { get; private set; }

    public string SourceKey { get; private set; }

    public string SourceOfferReference { get; private set; }

    public Instant QuotedAt { get; private set; }

    public Instant OfferExpiresAt { get; private set; }

    public Instant AcceptedAt { get; private set; }

    public string? Cabin { get; private set; }

    public string? BookingClass { get; private set; }

    public string? FareBasis { get; private set; }

    public string? FareFamily { get; private set; }

    public FlightBookingMonetarySnapshot Monetary { get; private set; }

    public FlightFareRulesSnapshot FareRules { get; private set; }

    public bool IsSameOfferIdentity(string sourceKey, string sourceOfferReference)
    {
        var key = NormalizeRequired(sourceKey, SourceKeyMaxLength, nameof(sourceKey)).ToLowerInvariant();
        var offerRef = NormalizeRequired(sourceOfferReference, SourceOfferReferenceMaxLength, nameof(sourceOfferReference));
        return SourceKey == key && SourceOfferReference == offerRef;
    }

    /// <summary>
    /// Accepts an authoritative source offer as the immutable transaction snapshot.
    /// Same offer identity and amount is idempotent; a different offer or silent repricing conflicts.
    /// </summary>
    public static FlightOfferSnapshot Accept(
        FlightBooking booking,
        Instant now,
        string sourceKey,
        string sourceOfferReference,
        Instant quotedAt,
        Instant offerExpiresAt,
        MoneyValue baseFare,
        MoneyValue taxes,
        MoneyValue fees,
        MoneyValue totalAmount,
        IReadOnlyList<FlightOfferSegmentIdentity> segments,
        FlightPassengerCount passengers,
        FlightFareRulesDraft fareRules,
        FlightOfferSnapshot? existingAccepted = null,
        MoneyValue? previouslyObservedTotal = null,
        IReadOnlyList<FlightPassengerCategoryFareLine>? categoryFares = null,
        IReadOnlyList<FlightBaggageAllowanceDraft>? baggage = null,
        string? cabin = null,
        string? bookingClass = null,
        string? fareBasis = null,
        string? fareFamily = null)
    {
        ArgumentNullException.ThrowIfNull(booking);
        ArgumentNullException.ThrowIfNull(baseFare);
        ArgumentNullException.ThrowIfNull(taxes);
        ArgumentNullException.ThrowIfNull(fees);
        ArgumentNullException.ThrowIfNull(totalAmount);
        ArgumentNullException.ThrowIfNull(segments);
        ArgumentNullException.ThrowIfNull(passengers);
        ArgumentNullException.ThrowIfNull(fareRules);
        EnsureClock(now, nameof(now));
        EnsureClock(quotedAt, nameof(quotedAt));
        EnsureClock(offerExpiresAt, nameof(offerExpiresAt));

        var normalizedKey = NormalizeRequired(sourceKey, SourceKeyMaxLength, nameof(sourceKey)).ToLowerInvariant();
        var normalizedOfferRef = NormalizeRequired(
            sourceOfferReference,
            SourceOfferReferenceMaxLength,
            nameof(sourceOfferReference));

        if (existingAccepted is not null)
        {
            if (existingAccepted.IsSameOfferIdentity(normalizedKey, normalizedOfferRef))
            {
                if (!existingAccepted.Monetary.Total.Equals(totalAmount))
                {
                    throw new InvalidOperationException(
                        "Silent repricing is forbidden; requote is required.");
                }

                return existingAccepted;
            }

            throw new InvalidOperationException(
                "A different flight offer is already accepted; requote is required.");
        }

        if (offerExpiresAt <= now)
        {
            throw new InvalidOperationException("Expired flight offer cannot be accepted.");
        }

        booking.EnsureMatchesCommercialOffer(booking.TripType, segments, passengers);

        RejectToman(totalAmount.Currency);
        RejectToman(baseFare.Currency);
        RejectToman(taxes.Currency);
        RejectToman(fees.Currency);
        EnsureSameCurrency(totalAmount, baseFare, nameof(baseFare));
        EnsureSameCurrency(totalAmount, taxes, nameof(taxes));
        EnsureSameCurrency(totalAmount, fees, nameof(fees));
        EnsureSameCurrency(totalAmount, previouslyObservedTotal, nameof(previouslyObservedTotal));

        var summed = baseFare.Add(taxes).Add(fees);
        if (summed.Amount != totalAmount.Amount || summed.Currency != totalAmount.Currency)
        {
            throw new ArgumentException(
                "TotalAmount must equal BaseFare + Taxes + Fees.",
                nameof(totalAmount));
        }

        if (previouslyObservedTotal is not null && !previouslyObservedTotal.Equals(totalAmount))
        {
            throw new InvalidOperationException(
                "Revalidated price differs from observed price; requote is required.");
        }

        var snapshotId = FlightOfferSnapshotId.New();
        if (normalizedOfferRef.Equals(snapshotId.ToString(), StringComparison.OrdinalIgnoreCase)
            || Guid.TryParse(normalizedOfferRef, out var asGuid) && asGuid == snapshotId.Value)
        {
            throw new ArgumentException(
                "SourceOfferReference must not equal FlightOfferSnapshotId.",
                nameof(sourceOfferReference));
        }

        var snapshot = new FlightOfferSnapshot(
            snapshotId,
            booking.Id,
            booking.TripType,
            normalizedKey,
            normalizedOfferRef,
            quotedAt,
            offerExpiresAt,
            now,
            NormalizeOptional(cabin, CabinMaxLength),
            NormalizeOptional(bookingClass, BookingClassMaxLength),
            NormalizeOptional(fareBasis, FareBasisMaxLength),
            NormalizeOptional(fareFamily, FareFamilyMaxLength));

        snapshot.Monetary = new FlightBookingMonetarySnapshot(
            snapshot.Id,
            booking.Id,
            CopyMoney(baseFare)!,
            CopyMoney(taxes)!,
            CopyMoney(fees)!,
            CopyMoney(totalAmount)!);

        AddCategoryFares(snapshot, booking, totalAmount, categoryFares);
        snapshot.FareRules = BuildFareRules(snapshot.Id, totalAmount, offerExpiresAt, fareRules, baggage);
        return snapshot;
    }

    private static void AddCategoryFares(
        FlightOfferSnapshot snapshot,
        FlightBooking booking,
        MoneyValue total,
        IReadOnlyList<FlightPassengerCategoryFareLine>? categoryFares)
    {
        if (categoryFares is null || categoryFares.Count == 0)
        {
            return;
        }

        var bookingCounts = booking.Passengers
            .GroupBy(p => p.Category)
            .ToDictionary(g => g.Key, g => g.Count());
        MoneyValue? sum = null;
        var seen = new HashSet<FlightPassengerCategory>();
        var ordinal = 1;
        foreach (var line in categoryFares)
        {
            ArgumentNullException.ThrowIfNull(line);
            ArgumentNullException.ThrowIfNull(line.Amount);
            if (!seen.Add(line.Category))
            {
                throw new ArgumentException("Duplicate passenger-category fare line.", nameof(categoryFares));
            }

            if (!bookingCounts.TryGetValue(line.Category, out var expectedCount)
                || expectedCount != line.PassengerCount)
            {
                throw new ArgumentException(
                    "Passenger-category fare coverage must match persisted FlightBooking.",
                    nameof(categoryFares));
            }

            EnsureSameCurrency(total, line.Amount, nameof(line.Amount));
            RejectToman(line.Amount.Currency);
            sum = sum is null ? line.Amount : sum.Add(line.Amount);
            snapshot.Monetary.AddCategoryFare(new FlightPassengerCategoryFareSnapshot(
                snapshot.Id,
                ordinal,
                line.Category,
                line.PassengerCount,
                CopyMoney(line.Amount)!));
            ordinal++;
        }

        if (seen.Count != bookingCounts.Count)
        {
            throw new ArgumentException(
                "Passenger-category fares must cover every passenger category when supplied.",
                nameof(categoryFares));
        }

        if (sum is null || sum.Amount != total.Amount)
        {
            throw new ArgumentException(
                "Passenger-category fare amounts must sum to TotalAmount when supplied.",
                nameof(categoryFares));
        }
    }

    private static FlightFareRulesSnapshot BuildFareRules(
        FlightOfferSnapshotId snapshotId,
        MoneyValue total,
        Instant offerExpiresAt,
        FlightFareRulesDraft fareRules,
        IReadOnlyList<FlightBaggageAllowanceDraft>? baggage)
    {
        if (fareRules.TicketingDeadline is { } deadline)
        {
            EnsureClock(deadline, nameof(fareRules.TicketingDeadline));
            if (deadline == offerExpiresAt)
            {
                throw new ArgumentException(
                    "TicketingDeadline must be distinct from OfferExpiresAt.",
                    nameof(fareRules));
            }
        }

        EnsurePenalty(total, fareRules.CancelPenalty, nameof(fareRules.CancelPenalty));
        EnsurePenalty(total, fareRules.ChangePenalty, nameof(fareRules.ChangePenalty));

        var snapshot = new FlightFareRulesSnapshot(
            snapshotId,
            fareRules.Refundable,
            fareRules.Changeable,
            fareRules.TicketingDeadline,
            CopyMoney(fareRules.CancelPenalty),
            CopyMoney(fareRules.ChangePenalty),
            fareRules.PartialRefundRequired);

        if (baggage is { Count: > 0 })
        {
            var ordinal = 1;
            foreach (var item in baggage)
            {
                ArgumentNullException.ThrowIfNull(item);
                if (item.Quantity is < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(baggage), "Baggage quantity cannot be negative.");
                }

                if (item.Weight is < 0m)
                {
                    throw new ArgumentOutOfRangeException(nameof(baggage), "Baggage weight cannot be negative.");
                }

                if (item.Quantity is null && item.Weight is null)
                {
                    throw new ArgumentException("Baggage allowance requires quantity or weight.", nameof(baggage));
                }

                if (item.PassengerCategory is { } category && !Enum.IsDefined(category))
                {
                    throw new ArgumentOutOfRangeException(nameof(baggage), category, "FlightPassengerCategory is not controlled.");
                }

                snapshot.AddBaggage(new FlightBaggageAllowanceSnapshot(
                    snapshotId,
                    ordinal,
                    item.Quantity,
                    item.Weight,
                    NormalizeOptional(item.Unit, FlightBaggageAllowanceSnapshot.UnitMaxLength),
                    NormalizeOptional(item.Category, FlightBaggageAllowanceSnapshot.CategoryMaxLength),
                    item.PassengerCategory));
                ordinal++;
            }
        }

        return snapshot;
    }

    private static void EnsurePenalty(MoneyValue total, MoneyValue? penalty, string paramName)
    {
        if (penalty is null)
        {
            return;
        }

        EnsureSameCurrency(total, penalty, paramName);
        RejectToman(penalty.Currency);
        if (penalty.Amount < 0m)
        {
            throw new ArgumentException("PenaltyAmount must be >= 0.", paramName);
        }

        if (penalty.Amount > total.Amount)
        {
            throw new ArgumentException("PenaltyAmount must be <= TotalAmount.", paramName);
        }
    }

    private static void RejectToman(CurrencyCode currency)
    {
        if (currency.Value.Equals("TOMAN", StringComparison.Ordinal))
        {
            throw new ArgumentException("Toman is not a CurrencyCode for FlightBooking monetary snapshots.");
        }
    }

    private static void EnsureSameCurrency(MoneyValue total, MoneyValue? other, string paramName)
    {
        if (other is not null && other.Currency != total.Currency)
        {
            throw new InvalidOperationException(
                "Monetary values on one accepted offer must share a single CurrencyCode; implicit FX is forbidden.");
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

using NodaTime;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Pricing.Domain;

/// <summary>
/// Quote aggregate owned by Pricing (TC-P12-T004 / P12-R4).
/// Quote = calculated price snapshot for a specific request (with expiration) —
/// not the live <see cref="Price"/>, not a Booking amount, and not Payment.
/// Ownership: Pricing → Quote → PriceSnapshot + Expiration.
/// Must not carry Customer / Passenger / Payment / Reservation / Booking fields.
/// </summary>
public sealed class Quote
{
    private readonly List<QuoteSnapshotComponent> _snapshotComponents = [];

    private Quote()
    {
        SnapshotTargetType = null;
    }

    private Quote(
        QuoteId id,
        PriceId sourcePriceId,
        PriceTargetType? snapshotTargetType,
        Guid? snapshotTargetId,
        Instant createdAt,
        Instant expiresAt)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("QuoteId cannot be empty.", nameof(id));
        }

        if (sourcePriceId.Value == Guid.Empty)
        {
            throw new ArgumentException("SourcePriceId cannot be empty.", nameof(sourcePriceId));
        }

        if (expiresAt <= createdAt)
        {
            throw new ArgumentException(
                "Quote expiration must be strictly after created-at.",
                nameof(expiresAt));
        }

        if (snapshotTargetType is not null && snapshotTargetId is null)
        {
            throw new ArgumentException(
                "SnapshotTargetId is required when SnapshotTargetType is set.",
                nameof(snapshotTargetId));
        }

        if (snapshotTargetId is Guid tid)
        {
            if (tid == Guid.Empty)
            {
                throw new ArgumentException("SnapshotTargetId cannot be empty.", nameof(snapshotTargetId));
            }

            if (snapshotTargetType is null)
            {
                throw new ArgumentException(
                    "SnapshotTargetType is required when SnapshotTargetId is set.",
                    nameof(snapshotTargetType));
            }
        }

        Id = id;
        SourcePriceId = sourcePriceId;
        SnapshotTargetType = snapshotTargetType;
        SnapshotTargetId = snapshotTargetId;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public QuoteId Id { get; private set; }

    /// <summary>
    /// Logical provenance to the live Price that was snapshotted — not an EF FK.
    /// Quote amounts never mutate when that Price later changes.
    /// </summary>
    public PriceId SourcePriceId { get; private set; }

    /// <summary>
    /// Optional copy of Price.TargetType at quote time (snapshot integrity; not a Tour FK).
    /// </summary>
    public PriceTargetType? SnapshotTargetType { get; private set; }

    /// <summary>
    /// Optional copy of Price.TargetId at quote time (snapshot integrity; logical Guid only).
    /// </summary>
    public Guid? SnapshotTargetId { get; private set; }

    public Instant CreatedAt { get; private set; }

    /// <summary>Required expiration instant; Quote is invalid at/after this moment.</summary>
    public Instant ExpiresAt { get; private set; }

    /// <summary>Immutable PriceSnapshot lines (kind + money) captured at quote time.</summary>
    public IReadOnlyCollection<QuoteSnapshotComponent> SnapshotComponents => _snapshotComponents;

    public IReadOnlyList<QuoteSnapshotComponent> SnapshotComponentsOrdered =>
        _snapshotComponents.OrderBy(x => x.SortOrder).ThenBy(x => x.Id.Value).ToList();

    /// <summary>Authoritative currency shared by every snapshot line on this Quote.</summary>
    public CurrencyCode Currency =>
        _snapshotComponents.Count == 0
            ? throw new InvalidOperationException("Quote has no snapshot components.")
            : _snapshotComponents[0].Money.Currency;

    /// <summary>
    /// Total derived from the immutable snapshot (same currency). Not a Booking Amount.
    /// </summary>
    public MoneyValue Total
    {
        get
        {
            if (_snapshotComponents.Count == 0)
            {
                throw new InvalidOperationException("Quote has no snapshot components.");
            }

            MoneyValue total = _snapshotComponents[0].Money;
            for (var i = 1; i < _snapshotComponents.Count; i++)
            {
                total = total.Add(_snapshotComponents[i].Money);
            }

            return total;
        }
    }

    public bool IsExpired(Instant now) => now >= ExpiresAt;

    /// <summary>
    /// Creates a Quote by snapshotting a live <see cref="Price"/> (components + optional target copy).
    /// Price ≠ Quote: the snapshot is frozen; later Price edits do not rewrite this Quote.
    /// </summary>
    public static Quote CreateFromPrice(Price price, Instant createdAt, Instant expiresAt)
    {
        ArgumentNullException.ThrowIfNull(price);

        var definitions = price.ComponentsOrdered
            .Select(c => new PriceComponentDefinition(c.Kind, c.Money, c.SortOrder, c.Code, c.Label))
            .ToList();

        return Create(
            price.Id,
            definitions,
            createdAt,
            expiresAt,
            price.TargetType,
            price.TargetId);
    }

    /// <summary>
    /// Creates a Quote with an explicit immutable PriceSnapshot.
    /// Requires non-empty snapshot, same currency across lines, and expiresAt &gt; createdAt.
    /// </summary>
    public static Quote Create(
        PriceId sourcePriceId,
        IReadOnlyList<PriceComponentDefinition> snapshotComponents,
        Instant createdAt,
        Instant expiresAt,
        PriceTargetType? snapshotTargetType = null,
        Guid? snapshotTargetId = null)
    {
        ArgumentNullException.ThrowIfNull(snapshotComponents);

        if (snapshotComponents.Count == 0)
        {
            throw new ArgumentException(
                "Quote cannot be created without a price snapshot.",
                nameof(snapshotComponents));
        }

        EnsureSnapshotDefinitionsValid(snapshotComponents);

        var quote = new Quote(
            QuoteId.New(),
            sourcePriceId,
            snapshotTargetType,
            snapshotTargetId,
            createdAt,
            expiresAt);

        foreach (var definition in snapshotComponents)
        {
            quote._snapshotComponents.Add(
                QuoteSnapshotComponent.Create(
                    quote.Id,
                    definition.Kind,
                    definition.Money,
                    definition.SortOrder,
                    definition.Code,
                    definition.Label));
        }

        return quote;
    }

    private static void EnsureSnapshotDefinitionsValid(IReadOnlyList<PriceComponentDefinition> components)
    {
        CurrencyCode? currency = null;
        var sortOrders = new HashSet<int>();
        var codes = new HashSet<string>(StringComparer.Ordinal);

        foreach (var definition in components)
        {
            ArgumentNullException.ThrowIfNull(definition.Money);

            if (currency is null)
            {
                currency = definition.Money.Currency;
            }
            else if (!definition.Money.Currency.Equals(currency))
            {
                // Quote snapshot inherits Price currency rules; FX is out of scope (P12-R5 deferred).
                throw new ArgumentException(
                    "All Quote snapshot components must share the same currency.",
                    nameof(components));
            }

            if (!sortOrders.Add(definition.SortOrder))
            {
                throw new ArgumentException(
                    $"Duplicate SortOrder {definition.SortOrder} within one Quote snapshot.",
                    nameof(components));
            }

            if (!string.IsNullOrWhiteSpace(definition.Code))
            {
                var normalized = definition.Code.Trim();
                if (!codes.Add(normalized))
                {
                    throw new ArgumentException(
                        $"Duplicate Code '{normalized}' within one Quote snapshot.",
                        nameof(components));
                }
            }
        }
    }
}

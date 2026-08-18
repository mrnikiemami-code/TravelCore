using NodaTime;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Immutable Booking-owned commercial snapshot copied from a valid Pricing Quote.
/// Transaction evidence — not a pricing engine and not a Payment amount.
/// </summary>
public sealed class BookingMonetarySnapshot
{
    private readonly List<BookingMonetaryComponent> _components = [];

    private BookingMonetarySnapshot()
    {
        Total = null!;
    }

    private BookingMonetarySnapshot(
        BookingMonetarySnapshotId id,
        BookingId bookingId,
        PricingQuoteReference quoteReference,
        Guid sourcePriceId,
        string? targetType,
        Guid? targetId,
        Instant quotedAt,
        Instant quoteExpiresAt,
        Instant acceptedAt,
        MoneyValue total)
    {
        Id = id;
        BookingId = bookingId;
        QuoteReference = quoteReference;
        SourcePriceId = sourcePriceId;
        TargetType = targetType;
        TargetId = targetId;
        QuotedAt = quotedAt;
        QuoteExpiresAt = quoteExpiresAt;
        AcceptedAt = acceptedAt;
        Total = total;
    }

    public BookingMonetarySnapshotId Id { get; private set; }

    public BookingId BookingId { get; private set; }

    public PricingQuoteReference QuoteReference { get; private set; }

    public Guid SourcePriceId { get; private set; }

    public string? TargetType { get; private set; }

    public Guid? TargetId { get; private set; }

    public Instant QuotedAt { get; private set; }

    public Instant QuoteExpiresAt { get; private set; }

    public Instant AcceptedAt { get; private set; }

    public MoneyValue Total { get; private set; }

    public IReadOnlyList<BookingMonetaryComponent> Components => _components;

    internal static BookingMonetarySnapshot CopyFrom(
        BookingId bookingId,
        AuthoritativeQuoteFacts facts,
        Instant acceptedAt)
    {
        ArgumentNullException.ThrowIfNull(facts);
        if (acceptedAt == default)
        {
            throw new ArgumentException("AcceptedAt cannot be default.", nameof(acceptedAt));
        }

        var snapshot = new BookingMonetarySnapshot(
            BookingMonetarySnapshotId.New(),
            bookingId,
            facts.QuoteReference,
            facts.SourcePriceId,
            facts.TargetType,
            facts.TargetId,
            facts.QuotedAt,
            facts.QuoteExpiresAt,
            acceptedAt,
            new MoneyValue(facts.Total.Amount, facts.Total.Currency));

        foreach (var fact in facts.Components.OrderBy(x => x.SortOrder))
        {
            snapshot._components.Add(BookingMonetaryComponent.CopyFrom(fact));
        }

        return snapshot;
    }
}

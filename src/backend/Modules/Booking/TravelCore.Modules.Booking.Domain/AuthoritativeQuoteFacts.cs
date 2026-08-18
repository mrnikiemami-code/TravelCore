using NodaTime;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Trusted Quote facts supplied by Pricing's read contract after mapping in application code.
/// Booking copies these values; it does not recalculate tax/fee/discount/total.
/// </summary>
public sealed class AuthoritativeQuoteFacts
{
    public const string ForbiddenTomanCode = "TOMAN";

    private AuthoritativeQuoteFacts(
        PricingQuoteReference quoteReference,
        Guid sourcePriceId,
        string? targetType,
        Guid? targetId,
        Instant quotedAt,
        Instant quoteExpiresAt,
        MoneyValue total,
        IReadOnlyList<AuthoritativeQuoteComponentFact> components)
    {
        QuoteReference = quoteReference;
        SourcePriceId = sourcePriceId;
        TargetType = targetType;
        TargetId = targetId;
        QuotedAt = quotedAt;
        QuoteExpiresAt = quoteExpiresAt;
        Total = total;
        Components = components;
    }

    public PricingQuoteReference QuoteReference { get; }

    public Guid SourcePriceId { get; }

    public string? TargetType { get; }

    public Guid? TargetId { get; }

    public Instant QuotedAt { get; }

    public Instant QuoteExpiresAt { get; }

    public MoneyValue Total { get; }

    public IReadOnlyList<AuthoritativeQuoteComponentFact> Components { get; }

    public bool IsExpired(Instant now) => now >= QuoteExpiresAt;

    public static AuthoritativeQuoteFacts Create(
        PricingQuoteReference quoteReference,
        Guid sourcePriceId,
        string? targetType,
        Guid? targetId,
        Instant quotedAt,
        Instant quoteExpiresAt,
        IReadOnlyList<AuthoritativeQuoteComponentFact> components)
    {
        ArgumentNullException.ThrowIfNull(components);
        if (quoteReference.LogicalId == Guid.Empty)
        {
            throw new ArgumentException("PricingQuoteReference cannot be empty.", nameof(quoteReference));
        }

        if (sourcePriceId == Guid.Empty)
        {
            throw new ArgumentException("SourcePriceId cannot be empty.", nameof(sourcePriceId));
        }

        if (quotedAt == default)
        {
            throw new ArgumentException("QuotedAt cannot be default.", nameof(quotedAt));
        }

        if (quoteExpiresAt <= quotedAt)
        {
            throw new ArgumentException("QuoteExpiresAt must be strictly after QuotedAt.", nameof(quoteExpiresAt));
        }

        if (components.Count == 0)
        {
            throw new ArgumentException("Authoritative Quote facts require snapshot components.", nameof(components));
        }

        if (!string.IsNullOrWhiteSpace(targetType) && targetId is null)
        {
            throw new ArgumentException("TargetId is required when TargetType is set.", nameof(targetId));
        }

        if (targetId is Guid tid)
        {
            if (tid == Guid.Empty)
            {
                throw new ArgumentException("TargetId cannot be empty.", nameof(targetId));
            }

            if (string.IsNullOrWhiteSpace(targetType))
            {
                throw new ArgumentException("TargetType is required when TargetId is set.", nameof(targetType));
            }
        }

        MoneyValue? total = null;
        var sortOrders = new HashSet<int>();
        foreach (var component in components)
        {
            ArgumentNullException.ThrowIfNull(component);
            ArgumentNullException.ThrowIfNull(component.Money);
            EnsureCanonicalCurrency(component.Money.Currency);
            if (!Enum.IsDefined(component.Kind))
            {
                throw new ArgumentOutOfRangeException(nameof(components), component.Kind, "Unsupported monetary component kind.");
            }

            if (!sortOrders.Add(component.SortOrder))
            {
                throw new ArgumentException($"Duplicate SortOrder {component.SortOrder} within Quote facts.", nameof(components));
            }

            total = total is null ? component.Money : total.Add(component.Money);
        }

        return new AuthoritativeQuoteFacts(
            quoteReference,
            sourcePriceId,
            string.IsNullOrWhiteSpace(targetType) ? null : targetType.Trim(),
            targetId,
            quotedAt,
            quoteExpiresAt,
            total!,
            components.ToArray());
    }

    internal static void EnsureCanonicalCurrency(CurrencyCode currency)
    {
        ArgumentNullException.ThrowIfNull(currency);
        if (currency.Value.Equals(ForbiddenTomanCode, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "TOMAN is not a canonical CurrencyCode; store IRR and convert only at explicit display/input boundaries (ADR 0003).",
                nameof(currency));
        }
    }
}

public sealed record AuthoritativeQuoteComponentFact(
    BookingMonetaryComponentKind Kind,
    MoneyValue Money,
    int SortOrder,
    string? Code,
    string? Label);

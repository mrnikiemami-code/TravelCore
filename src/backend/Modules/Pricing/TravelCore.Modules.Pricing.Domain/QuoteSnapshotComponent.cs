using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Pricing.Domain;

/// <summary>
/// Immutable structured line in a Quote price snapshot (kind + money) — not an opaque blob.
/// Captured at quote time; never mutates when the live <see cref="Price"/> changes.
/// </summary>
public sealed class QuoteSnapshotComponent
{
    public const int CodeMaxLength = 64;
    public const int LabelMaxLength = 200;

    private QuoteSnapshotComponent()
    {
        Money = null!;
        Code = null;
        Label = null;
    }

    private QuoteSnapshotComponent(
        QuoteSnapshotComponentId id,
        QuoteId quoteId,
        PriceComponentKind kind,
        MoneyValue money,
        int sortOrder,
        string? code,
        string? label)
    {
        Id = id;
        QuoteId = quoteId;
        Kind = kind;
        Money = money;
        SortOrder = sortOrder;
        Code = code;
        Label = label;
    }

    public QuoteSnapshotComponentId Id { get; private set; }

    public QuoteId QuoteId { get; private set; }

    public PriceComponentKind Kind { get; private set; }

    /// <summary>Platform money value frozen at quote time (required currency; one code per amount).</summary>
    public MoneyValue Money { get; private set; }

    public int SortOrder { get; private set; }

    public string? Code { get; private set; }

    public string? Label { get; private set; }

    internal static QuoteSnapshotComponent Create(
        QuoteId quoteId,
        PriceComponentKind kind,
        MoneyValue money,
        int sortOrder,
        string? code,
        string? label)
    {
        if (quoteId.Value == Guid.Empty)
        {
            throw new ArgumentException("QuoteId cannot be empty.", nameof(quoteId));
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported PriceComponentKind.");
        }

        ArgumentNullException.ThrowIfNull(money);
        PricingCurrency.EnsureCanonical(money.Currency);

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, "SortOrder must be >= 0.");
        }

        return new QuoteSnapshotComponent(
            QuoteSnapshotComponentId.New(),
            quoteId,
            kind,
            money,
            sortOrder,
            NormalizeOptional(code, CodeMaxLength, nameof(code)),
            NormalizeOptional(label, LabelMaxLength, nameof(label)));
    }

    private static string? NormalizeOptional(string? value, int maxLength, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"Max length is {maxLength}.", paramName);
        }

        return trimmed;
    }
}

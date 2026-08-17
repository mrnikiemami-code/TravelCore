using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Pricing.Domain;

/// <summary>
/// Structured monetary component of a <see cref="Price"/> (Base / Fee / Tax) — not an opaque blob.
/// Amount uses platform <see cref="MoneyValue"/> via <see cref="PricingMoney"/> (P12-R2).
/// </summary>
public sealed class PriceComponent
{
    public const int CodeMaxLength = 64;
    public const int LabelMaxLength = 200;

    private PriceComponent()
    {
        Money = null!;
        Code = null;
        Label = null;
    }

    private PriceComponent(
        PriceComponentId id,
        PriceId priceId,
        PriceComponentKind kind,
        MoneyValue money,
        int sortOrder,
        string? code,
        string? label)
    {
        Id = id;
        PriceId = priceId;
        Kind = kind;
        Money = money;
        SortOrder = sortOrder;
        Code = code;
        Label = label;
    }

    public PriceComponentId Id { get; private set; }

    public PriceId PriceId { get; private set; }

    public PriceComponentKind Kind { get; private set; }

    /// <summary>Platform money value (required currency; one code per amount).</summary>
    public MoneyValue Money { get; private set; }

    public int SortOrder { get; private set; }

    /// <summary>Optional stable code within a Price (uniqueness when present).</summary>
    public string? Code { get; private set; }

    public string? Label { get; private set; }

    internal static PriceComponent Create(
        PriceId priceId,
        PriceComponentKind kind,
        MoneyValue money,
        int sortOrder,
        string? code,
        string? label)
    {
        if (priceId.Value == Guid.Empty)
        {
            throw new ArgumentException("PriceId cannot be empty.", nameof(priceId));
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

        return new PriceComponent(
            PriceComponentId.New(),
            priceId,
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

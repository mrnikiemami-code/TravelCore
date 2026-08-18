using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Booking-owned copy of one Quote snapshot line. New identity — not a Pricing-schema FK.
/// </summary>
public sealed class BookingMonetaryComponent
{
    public const int CodeMaxLength = 64;
    public const int LabelMaxLength = 200;

    private BookingMonetaryComponent()
    {
        Money = null!;
    }

    private BookingMonetaryComponent(
        BookingMonetaryComponentId id,
        BookingMonetaryComponentKind kind,
        MoneyValue money,
        int sortOrder,
        string? code,
        string? label)
    {
        Id = id;
        Kind = kind;
        Money = money;
        SortOrder = sortOrder;
        Code = code;
        Label = label;
    }

    public BookingMonetaryComponentId Id { get; private set; }

    public BookingMonetaryComponentKind Kind { get; private set; }

    public MoneyValue Money { get; private set; }

    public int SortOrder { get; private set; }

    public string? Code { get; private set; }

    public string? Label { get; private set; }

    internal static BookingMonetaryComponent CopyFrom(AuthoritativeQuoteComponentFact fact)
    {
        ArgumentNullException.ThrowIfNull(fact);
        ArgumentNullException.ThrowIfNull(fact.Money);
        AuthoritativeQuoteFacts.EnsureCanonicalCurrency(fact.Money.Currency);
        if (fact.SortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fact), fact.SortOrder, "SortOrder must be >= 0.");
        }

        return new BookingMonetaryComponent(
            BookingMonetaryComponentId.New(),
            fact.Kind,
            new MoneyValue(fact.Money.Amount, fact.Money.Currency),
            fact.SortOrder,
            NormalizeOptional(fact.Code, CodeMaxLength, nameof(fact.Code)),
            NormalizeOptional(fact.Label, LabelMaxLength, nameof(fact.Label)));
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

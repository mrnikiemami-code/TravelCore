namespace TravelCore.Money;

/// <summary>
/// Immutable monetary primitive: decimal amount + explicit <see cref="CurrencyCode"/> (ADR 0003).
/// Not Pricing — no FX, discounts, or business price concepts.
/// </summary>
public sealed class Money : IEquatable<Money>, IComparable<Money>
{
    public Money(decimal amount, CurrencyCode currency)
    {
        ArgumentNullException.ThrowIfNull(currency);
        Amount = amount;
        Currency = currency;
    }

    public Money(decimal amount, string currencyCode)
        : this(amount, CurrencyCode.Parse(currencyCode))
    {
    }

    public decimal Amount { get; }

    public CurrencyCode Currency { get; }

    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public static Money operator +(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);
        return left.Add(right);
    }

    public static Money operator -(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);
        return left.Subtract(right);
    }

    public int CompareTo(Money? other)
    {
        if (other is null)
        {
            return 1;
        }

        EnsureSameCurrency(other);
        return Amount.CompareTo(other.Amount);
    }

    public bool Equals(Money? other) =>
        other is not null && Amount == other.Amount && Currency == other.Currency;

    public override bool Equals(object? obj) => obj is Money other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    public override string ToString() => $"{Amount} {Currency.Value}";

    public static bool operator ==(Money? left, Money? right) => Equals(left, right);

    public static bool operator !=(Money? left, Money? right) => !Equals(left, right);

    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;

    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;

    public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;

    public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
        {
            // ارزهای متفاوت عمداً ترکیب/مقایسه نمی‌شوند؛ FX فقط در مرز Pricing با نرخ صریح.
            throw new InvalidOperationException(
                $"Cannot combine or compare Money values across currencies ({Currency.Value} vs {other.Currency.Value}).");
        }
    }
}

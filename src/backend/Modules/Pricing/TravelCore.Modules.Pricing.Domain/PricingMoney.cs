using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Pricing.Domain;

/// <summary>
/// Pricing money factory over platform <see cref="MoneyValue"/> (ADR 0003 / P12-R2).
/// Reuses TravelCore.Money — does not invent a parallel money type.
/// Each value has exactly one authoritative <see cref="CurrencyCode"/>; twin SoR duplicates are forbidden.
/// FX conversion, Quote conversion, and Payment currency are out of scope for this baseline.
/// </summary>
public static class PricingMoney
{
    /// <summary>
    /// Creates a Pricing money value: currency required; amount rules follow platform Money ADR.
    /// </summary>
    public static MoneyValue Create(decimal amount, string currencyCode) =>
        new(amount, PricingCurrency.ParseRequired(currencyCode));

    /// <summary>
    /// Creates a Pricing money value from an already-canonical currency.
    /// </summary>
    public static MoneyValue Create(decimal amount, CurrencyCode currency)
    {
        ArgumentNullException.ThrowIfNull(currency);
        PricingCurrency.EnsureCanonical(currency);
        return new MoneyValue(amount, currency);
    }
}

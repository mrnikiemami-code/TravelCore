using TravelCore.Money;

namespace TravelCore.Modules.Pricing.Domain;

/// <summary>
/// Pricing currency posture (P12-R2 / ADR 0003).
/// Currency is always required and canonical; Pricing does not invent parallel currency types.
/// </summary>
public static class PricingCurrency
{
    /// <summary>Forbidden stored currency — Toman is display/input only (1 Toman = 10 IRR).</summary>
    public const string ForbiddenTomanCode = "TOMAN";

    /// <summary>
    /// Parses and validates a required currency for Pricing money values.
    /// </summary>
    public static CurrencyCode ParseRequired(string? currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            throw new ArgumentException(
                "Currency is required for Pricing money values.",
                nameof(currencyCode));
        }

        var parsed = CurrencyCode.Parse(currencyCode);
        EnsureCanonical(parsed);
        return parsed;
    }

    /// <summary>
    /// Rejects non-canonical stored codes (notably TOMAN). Platform Parse alone accepts A–Z length rules only.
    /// </summary>
    public static void EnsureCanonical(CurrencyCode currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        if (currency.Value.Equals(ForbiddenTomanCode, StringComparison.Ordinal))
        {
            // تومان واحد نمایش/ورودی است؛ ارز کاننیکال ذخیره‌شده برای ریال ایران IRR است (ADR 0003).
            throw new ArgumentException(
                "TOMAN is not a canonical CurrencyCode; store IRR and convert only at explicit display/input boundaries (ADR 0003).",
                nameof(currency));
        }
    }
}

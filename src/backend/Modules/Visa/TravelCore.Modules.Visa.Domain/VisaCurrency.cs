using TravelCore.Money;

namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Visa currency posture for official fee facts (TC-P17-T006 / ADR 0003).
/// Reuses platform CurrencyCode. Does not invent a second Money type or perform FX.
/// </summary>
public static class VisaCurrency
{
    public const string ForbiddenTomanCode = "TOMAN";

    public static CurrencyCode ParseRequired(string? currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            throw new ArgumentException("Currency is required for official visa fee facts.", nameof(currencyCode));
        }

        var parsed = CurrencyCode.Parse(currencyCode);
        EnsureCanonical(parsed);
        return parsed;
    }

    public static void EnsureCanonical(CurrencyCode currency)
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

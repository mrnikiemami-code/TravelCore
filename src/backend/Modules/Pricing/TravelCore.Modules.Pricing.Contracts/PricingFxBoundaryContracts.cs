namespace TravelCore.Modules.Pricing.Contracts;

/// <summary>
/// Currency context for a Quote request (TC-P12-T007 / P12-R7).
/// SourceCurrency is the authoritative price/snapshot currency.
/// RequestedDisplayCurrency is optional metadata only — not a converted amount and not a second SoR.
/// Pricing does not convert; future FX Service owns ExchangeRate + Conversion.
/// </summary>
public sealed record QuoteCurrencyContext(
    string SourceCurrency,
    string? RequestedDisplayCurrency);

/// <summary>
/// Future FX boundary: Pricing may *request* conversion later.
/// P12 implementations must not calculate rates or return converted money.
/// </summary>
public interface IFxConversionPort
{
    /// <summary>
    /// Requests display conversion for a Quote currency context.
    /// P12: must fail closed — FX is not available; exchange-rate ownership is not Pricing.
    /// </summary>
    Task RequestDisplayConversionAsync(
        QuoteCurrencyContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Fail-closed FX boundary result for P12. Not a conversion calculation type.
/// </summary>
public sealed class FxBoundaryUnavailableException : NotSupportedException
{
    public const string DefaultMessage =
        "FX conversion is not available in P12. Pricing keeps the price currency and does not convert. Exchange-rate ownership is not Pricing; future FX Service owns ExchangeRate + Conversion.";

    public FxBoundaryUnavailableException()
        : base(DefaultMessage)
    {
    }

    public FxBoundaryUnavailableException(string message)
        : base(message)
    {
    }
}

using TravelCore.Modules.Pricing.Contracts;

namespace TravelCore.Modules.Pricing.Infrastructure;

/// <summary>
/// P12 FX boundary stub (TC-P12-T007 / P12-R7). Does not load rates and does not calculate.
/// Pricing may call this port later; today it fail-closes.
/// </summary>
internal sealed class FxBoundaryUnavailablePort : IFxConversionPort
{
    public Task RequestDisplayConversionAsync(
        QuoteCurrencyContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        throw new FxBoundaryUnavailableException();
    }
}

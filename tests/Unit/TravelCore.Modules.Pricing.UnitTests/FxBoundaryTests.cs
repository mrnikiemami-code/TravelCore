using TravelCore.Modules.Pricing.Contracts;
using TravelCore.Modules.Pricing.Infrastructure;
using Xunit;

namespace TravelCore.Modules.Pricing.UnitTests;

/// <summary>
/// P12-R7 FX boundary: request shape exists; Pricing does not calculate conversion.
/// </summary>
public sealed class FxBoundaryTests
{
    [Fact]
    public void QuoteCurrencyContext_Records_Source_And_Optional_Display_Without_Amounts()
    {
        var none = new QuoteCurrencyContext("USD", RequestedDisplayCurrency: null);
        var requested = new QuoteCurrencyContext("USD", "IRR");

        Assert.Equal("USD", none.SourceCurrency);
        Assert.Null(none.RequestedDisplayCurrency);
        Assert.Equal("IRR", requested.RequestedDisplayCurrency);

        Assert.Null(typeof(QuoteCurrencyContext).GetProperty("ConvertedAmount"));
        Assert.Null(typeof(QuoteCurrencyContext).GetProperty("Rate"));
        Assert.Null(typeof(QuoteCurrencyContext).GetProperty("TargetAmount"));
    }

    [Fact]
    public void FxBoundaryUnavailablePort_Throws_Without_Calculating()
    {
        IFxConversionPort port = new FxBoundaryUnavailablePort();

        var ex = Assert.Throws<FxBoundaryUnavailableException>(() =>
            port.RequestDisplayConversionAsync(
                    new QuoteCurrencyContext("USD", "EUR"),
                    TestContext.Current.CancellationToken)
                .GetAwaiter()
                .GetResult());

        Assert.IsAssignableFrom<NotSupportedException>(ex);
        Assert.Contains("not available in P12", ex.Message, StringComparison.Ordinal);
        Assert.Contains("does not convert", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void FxBoundaryUnavailablePort_Rejects_Null_Context()
    {
        var port = new FxBoundaryUnavailablePort();
        Assert.Throws<ArgumentNullException>(() =>
            port.RequestDisplayConversionAsync(null!, TestContext.Current.CancellationToken).GetAwaiter().GetResult());
    }
}

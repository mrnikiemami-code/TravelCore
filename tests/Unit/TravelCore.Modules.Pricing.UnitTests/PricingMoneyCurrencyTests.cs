using TravelCore.Modules.Pricing.Domain;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;
using Xunit;

namespace TravelCore.Modules.Pricing.UnitTests;

/// <summary>
/// Money / currency baseline rules for Pricing (TC-P12-T002 / P12-R2 / ADR 0003).
/// </summary>
public sealed class PricingMoneyCurrencyTests
{
    [Fact]
    public void Create_Requires_Currency_And_Stores_Single_Authoritative_Code()
    {
        var money = PricingMoney.Create(1290m, "usd");

        Assert.Equal(1290m, money.Amount);
        Assert.Equal("USD", money.Currency.Value);
    }

    [Fact]
    public void Create_Accepts_Irr_And_Allows_Negative_Amount_Per_Money_Adr()
    {
        var money = PricingMoney.Create(-10.5m, "IRR");

        Assert.Equal(-10.5m, money.Amount);
        Assert.Equal("IRR", money.Currency.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseRequired_Rejects_Missing_Currency(string? code)
    {
        var ex = Assert.Throws<ArgumentException>(() => PricingCurrency.ParseRequired(code));
        Assert.Equal("currencyCode", ex.ParamName);
    }

    [Fact]
    public void ParseRequired_Rejects_Toman_As_Stored_Currency()
    {
        var ex = Assert.Throws<ArgumentException>(() => PricingCurrency.ParseRequired("toman"));
        Assert.Contains("TOMAN", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Create_Rejects_Toman()
    {
        Assert.Throws<ArgumentException>(() => PricingMoney.Create(1000m, "TOMAN"));
    }

    [Fact]
    public void Create_From_CurrencyCode_Rejects_Toman()
    {
        var toman = CurrencyCode.Parse("TOMAN");
        Assert.Throws<ArgumentException>(() => PricingMoney.Create(1000m, toman));
    }

    [Fact]
    public void Platform_Money_Rejects_Cross_Currency_Combine()
    {
        var usd = PricingMoney.Create(10m, "USD");
        var irr = PricingMoney.Create(100m, "IRR");

        Assert.Throws<InvalidOperationException>(() => usd.Add(irr));
    }

    [Fact]
    public void Same_Currency_Add_Works_Without_Fx()
    {
        var a = PricingMoney.Create(10m, "USD");
        var b = PricingMoney.Create(2.5m, "USD");

        var sum = a.Add(b);

        Assert.Equal(12.5m, sum.Amount);
        Assert.Equal("USD", sum.Currency.Value);
    }

    [Fact]
    public void Pricing_Reuses_Platform_Money_Type()
    {
        var money = PricingMoney.Create(1m, "EUR");
        Assert.IsType<MoneyValue>(money);
        Assert.Equal("TravelCore.Money", typeof(MoneyValue).Assembly.GetName().Name);
    }
}

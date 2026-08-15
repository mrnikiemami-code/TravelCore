using TravelCore.Modules.ReferenceData.Domain;
using Xunit;

namespace TravelCore.Modules.ReferenceData.UnitTests;

public sealed class ReferenceDataCatalogEntryTests
{
    [Fact]
    public void Currency_Create_NormalizesCodeAndRejectsInvalidCodes()
    {
        var usd = CurrencyCatalogEntry.Create(" usd ", "US Dollar", 2, "$");
        Assert.Equal("USD", usd.Code);
        Assert.Equal(2, usd.MinorUnits);

        Assert.Throws<ArgumentException>(() => CurrencyCatalogEntry.Create("US", "Too short", 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => CurrencyCatalogEntry.Create("USD", "US Dollar", 9));
    }

    [Fact]
    public void Locale_Create_CanonicalizesLanguageAndRegion()
    {
        var locale = LocaleCatalogEntry.Create("FA-ir", "Persian (Iran)");
        Assert.Equal("fa-IR", locale.Code);
    }

    [Fact]
    public void Country_Create_NormalizesIsoCodes()
    {
        var country = CountryCatalogEntry.Create("ir", "irn", "Iran", "364");
        Assert.Equal("IR", country.Alpha2Code);
        Assert.Equal("IRN", country.Alpha3Code);
        Assert.Equal("364", country.NumericCode);
    }

    [Fact]
    public void TimeZone_Create_ValidatesAgainstTzdb()
    {
        var zone = TimeZoneCatalogEntry.Create("Asia/Tehran", "Iran Standard Time");
        Assert.Equal("Asia/Tehran", zone.Id);
        Assert.Throws<ArgumentException>(() => TimeZoneCatalogEntry.Create("Not/AZone", "x"));
    }
}

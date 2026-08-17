using NodaTime;
using TravelCore.Modules.Visa.Contracts;
using TravelCore.Modules.Visa.Domain;
using Xunit;

namespace TravelCore.Modules.Visa.UnitTests;

/// <summary>
/// OfficialVisaFee != CommercialPrice / Quote (TC-P17-T006 / P17-R6).
/// </summary>
public sealed class VisaOfficialFeeTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 23, 0);
    private static readonly Guid France = Guid.Parse("0198b3e0-0000-7000-8000-000000000061");

    private static VisaRequirementSet CreateSet() =>
        VisaDefinition.Create("TOURIST", "en", "Tourist", Now).AddRequirementSet(France, Now, "ir");

    [Fact]
    public void OfficialFee_Is_Regulatory_Fact_Not_Commercial_Price_Or_Quote()
    {
        var set = CreateSet();
        var embassy = set.AddOfficialFee("Embassy", 80m, "eur", Now, "  consular schedule  ", 1);
        set.AddOfficialFee("ServiceCenter", 20m, "EUR", Now);

        Assert.Equal(2, set.OfficialFees.Count);
        Assert.Equal(VisaOfficialFeeKind.Embassy, embassy.Kind);
        Assert.Equal(80m, embassy.Money.Amount);
        Assert.Equal("EUR", embassy.Money.Currency.Value);
        Assert.Equal("consular schedule", embassy.Source);
        Assert.Throws<InvalidOperationException>(() => set.AddOfficialFee("Embassy", 1m, "EUR", Now));
        Assert.Throws<ArgumentException>(() => set.AddOfficialFee("Markup", 1m, "EUR", Now));
        Assert.Throws<ArgumentException>(() => set.AddOfficialFee("Commission", 1m, "EUR", Now));
        Assert.Throws<ArgumentException>(() => set.AddOfficialFee("Discount", 1m, "EUR", Now));
        Assert.Throws<ArgumentException>(() => set.AddOfficialFee("Issuance", 10m, "TOMAN", Now));
        Assert.Throws<ArgumentOutOfRangeException>(() => set.AddOfficialFee("Application", -1m, "EUR", Now));
        Assert.Null(typeof(VisaOfficialFee).GetProperty("Quote"));
        Assert.Null(typeof(VisaOfficialFee).GetProperty("Discount"));
        Assert.Null(typeof(VisaOfficialFee).GetProperty("Commission"));
        Assert.Null(typeof(VisaOfficialFee).GetProperty("Markup"));
        Assert.Null(typeof(VisaRequirementSet).GetProperty("TotalPrice"));
        Assert.Null(typeof(VisaRequirementSet).GetProperty("Price"));
        Assert.Null(typeof(VisaOfficialFee).GetProperty("ExchangeRate"));
        Assert.True(VisaOwnershipBoundary.FeeModelImplemented);
        Assert.False(VisaOwnershipBoundary.OwnsPricing);
        Assert.False(VisaOwnershipBoundary.OwnsQuote);
        Assert.False(VisaOwnershipBoundary.ApplicationWorkflowImplemented);
        Assert.NotEqual("Price", typeof(VisaOfficialFee).Name);
    }
}

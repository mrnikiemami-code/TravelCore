using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Pricing.Infrastructure.Persistence;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;
using Xunit;

namespace TravelCore.Modules.Pricing.UnitTests;

/// <summary>
/// Verifies EF owned Money mapping pattern (Amount + CurrencyCode) without a product Price aggregate.
/// </summary>
public sealed class MoneyOwnedMappingTests
{
    [Fact]
    public void OwnsRequiredMoney_Maps_Amount_Numeric_And_CurrencyCode_String()
    {
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<MoneyHost>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.OwnsRequiredMoney(x => x.Value, "amount", "currency_code");
        });

        var model = modelBuilder.FinalizeModel();
        var host = model.FindEntityType(typeof(MoneyHost));
        Assert.NotNull(host);

        var moneyNav = host.FindNavigation(nameof(MoneyHost.Value));
        Assert.NotNull(moneyNav);
        Assert.False(moneyNav.IsCollection);
        Assert.True(moneyNav.ForeignKey.IsRequired);

        var moneyType = moneyNav.TargetEntityType;
        var amount = moneyType.FindProperty(nameof(MoneyValue.Amount));
        var currency = moneyType.FindProperty(nameof(MoneyValue.Currency));

        Assert.NotNull(amount);
        Assert.NotNull(currency);
        Assert.Equal("amount", amount.FindAnnotation("Relational:ColumnName")?.Value);
        Assert.Equal(
            MoneyOwnedMapping.DefaultAmountColumnType,
            amount.FindAnnotation("Relational:ColumnType")?.Value);
        Assert.Equal("currency_code", currency.FindAnnotation("Relational:ColumnName")?.Value);
        Assert.Equal(CurrencyCode.MaxLength, currency.GetMaxLength());
        Assert.NotNull(currency.GetValueConverter());
    }

    private sealed class MoneyHost
    {
        public Guid Id { get; private set; }
        public MoneyValue Value { get; private set; } = null!;
    }
}

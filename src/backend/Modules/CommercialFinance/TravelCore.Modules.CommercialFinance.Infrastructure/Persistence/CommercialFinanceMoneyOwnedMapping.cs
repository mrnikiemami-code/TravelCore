using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.CommercialFinance.Infrastructure.Persistence;

/// <summary>
/// EF owned mapping for optional platform Money on Commercial Finance facts.
/// Copied locally — do not reference Pricing.Infrastructure or Booking.Infrastructure.
/// </summary>
internal static class CommercialFinanceMoneyOwnedMapping
{
    public const string DefaultAmountColumnType = "numeric(24,8)";

    public static void OwnsOptionalMoney<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, MoneyValue?>> navigation,
        string amountColumnName,
        string currencyColumnName)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(navigation);
        ArgumentException.ThrowIfNullOrWhiteSpace(amountColumnName);
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyColumnName);

        builder.OwnsOne(navigation, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName(amountColumnName)
                .HasColumnType(DefaultAmountColumnType);

            money.Property(m => m.Currency)
                .HasColumnName(currencyColumnName)
                .HasMaxLength(CurrencyCode.MaxLength)
                .HasConversion(
                    code => code.Value,
                    value => CurrencyCode.Parse(value));
        });
    }
}

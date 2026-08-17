using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Pricing.Infrastructure.Persistence;

/// <summary>
/// EF owned mapping helper for platform <see cref="MoneyValue"/> → Amount + CurrencyCode columns.
/// P12-R2: one authoritative currency per money value; ADR 0003 default precision <c>numeric(24,8)</c>.
/// Used by <see cref="PriceComponentConfiguration"/> — not an FX or Quote mapping.
/// </summary>
public static class MoneyOwnedMapping
{
    public const string DefaultAmountColumnType = "numeric(24,8)";

    /// <summary>
    /// Maps a required owned <see cref="MoneyValue"/> as two columns on the owner table.
    /// </summary>
    public static void OwnsRequiredMoney<TEntity>(
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
                .HasColumnType(DefaultAmountColumnType)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName(currencyColumnName)
                .HasMaxLength(CurrencyCode.MaxLength)
                .HasConversion(
                    code => code.Value,
                    value => CurrencyCode.Parse(value))
                .IsRequired();
        });

        builder.Navigation(navigation).IsRequired();
    }
}

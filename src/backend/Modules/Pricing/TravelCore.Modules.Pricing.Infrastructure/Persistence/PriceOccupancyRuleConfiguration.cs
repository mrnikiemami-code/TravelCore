using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Pricing.Domain;

namespace TravelCore.Modules.Pricing.Infrastructure.Persistence;

internal sealed class PriceOccupancyRuleConfiguration : IEntityTypeConfiguration<PriceOccupancyRule>
{
    public void Configure(EntityTypeBuilder<PriceOccupancyRule> builder)
    {
        builder.ToTable("price_occupancy_rules");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => PriceOccupancyRuleId.From(value));

        builder.Property(x => x.PriceId)
            .HasColumnName("price_id")
            .HasConversion(id => id.Value, value => PriceId.From(value))
            .IsRequired();

        builder.Property(x => x.MarketPriceType)
            .HasColumnName("market_price_type")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.PassengerCategory)
            .HasColumnName("passenger_category")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.OccupancyCategory)
            .HasColumnName("occupancy_category")
            .HasConversion<short>()
            .IsRequired();

        builder.OwnsRequiredMoney(x => x.Money, "amount", "currency_code");

        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.HasIndex(x => new { x.PriceId, x.SortOrder })
            .IsUnique()
            .HasDatabaseName("ux_price_occupancy_rules_price_sort_order");

        builder.HasIndex(x => new
            {
                x.PriceId,
                x.MarketPriceType,
                x.PassengerCategory,
                x.OccupancyCategory
            })
            .IsUnique()
            .HasDatabaseName("ux_price_occupancy_rules_price_market_passenger_occupancy");
    }
}

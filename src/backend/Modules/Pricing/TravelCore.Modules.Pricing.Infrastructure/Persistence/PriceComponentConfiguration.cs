using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Pricing.Domain;

namespace TravelCore.Modules.Pricing.Infrastructure.Persistence;

internal sealed class PriceComponentConfiguration : IEntityTypeConfiguration<PriceComponent>
{
    public void Configure(EntityTypeBuilder<PriceComponent> builder)
    {
        builder.ToTable("price_components");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => PriceComponentId.From(value));

        builder.Property(x => x.PriceId)
            .HasColumnName("price_id")
            .HasConversion(id => id.Value, value => PriceId.From(value))
            .IsRequired();

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasConversion<short>()
            .IsRequired();

        builder.OwnsRequiredMoney(x => x.Money, "amount", "currency_code");

        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(PriceComponent.CodeMaxLength);

        builder.Property(x => x.Label)
            .HasColumnName("label")
            .HasMaxLength(PriceComponent.LabelMaxLength);

        builder.HasIndex(x => new { x.PriceId, x.SortOrder })
            .IsUnique()
            .HasDatabaseName("ux_price_components_price_sort_order");

        builder.HasIndex(x => new { x.PriceId, x.Code })
            .IsUnique()
            .HasDatabaseName("ux_price_components_price_code")
            .HasFilter("code IS NOT NULL");
    }
}

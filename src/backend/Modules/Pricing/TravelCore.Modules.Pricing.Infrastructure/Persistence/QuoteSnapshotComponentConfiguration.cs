using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Pricing.Domain;

namespace TravelCore.Modules.Pricing.Infrastructure.Persistence;

internal sealed class QuoteSnapshotComponentConfiguration : IEntityTypeConfiguration<QuoteSnapshotComponent>
{
    public void Configure(EntityTypeBuilder<QuoteSnapshotComponent> builder)
    {
        builder.ToTable("quote_snapshot_components");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => QuoteSnapshotComponentId.From(value));

        builder.Property(x => x.QuoteId)
            .HasColumnName("quote_id")
            .HasConversion(id => id.Value, value => QuoteId.From(value))
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
            .HasMaxLength(QuoteSnapshotComponent.CodeMaxLength);

        builder.Property(x => x.Label)
            .HasColumnName("label")
            .HasMaxLength(QuoteSnapshotComponent.LabelMaxLength);

        builder.HasIndex(x => new { x.QuoteId, x.SortOrder })
            .IsUnique()
            .HasDatabaseName("ux_quote_snapshot_components_quote_sort_order");

        builder.HasIndex(x => new { x.QuoteId, x.Code })
            .IsUnique()
            .HasDatabaseName("ux_quote_snapshot_components_quote_code")
            .HasFilter("code IS NOT NULL");
    }
}

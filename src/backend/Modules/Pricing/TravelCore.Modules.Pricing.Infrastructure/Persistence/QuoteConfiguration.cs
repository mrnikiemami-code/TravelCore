using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Pricing.Domain;

namespace TravelCore.Modules.Pricing.Infrastructure.Persistence;

internal sealed class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.ToTable("quotes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => QuoteId.From(value));

        // Logical provenance only — no FK to prices so snapshots survive live Price edits/deletes (P12-R4).
        builder.Property(x => x.SourcePriceId)
            .HasColumnName("source_price_id")
            .HasConversion(id => id.Value, value => PriceId.From(value))
            .IsRequired();

        builder.Property(x => x.SnapshotTargetType)
            .HasColumnName("snapshot_target_type")
            .HasMaxLength(PriceTargetType.MaxLength)
            .HasConversion(
                type => type == null ? null : type.Value,
                value => value == null ? null : PriceTargetType.Parse(value));

        builder.Property(x => x.SnapshotTargetId)
            .HasColumnName("snapshot_target_id");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Ignore(x => x.Currency);
        builder.Ignore(x => x.Total);
        builder.Ignore(x => x.SnapshotComponentsOrdered);

        builder.HasMany(x => x.SnapshotComponents)
            .WithOne()
            .HasForeignKey(x => x.QuoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.SnapshotComponents)
            .HasField("_snapshotComponents")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasIndex(x => x.SourcePriceId)
            .HasDatabaseName("ix_quotes_source_price_id");

        builder.HasIndex(x => x.ExpiresAt)
            .HasDatabaseName("ix_quotes_expires_at");
    }
}

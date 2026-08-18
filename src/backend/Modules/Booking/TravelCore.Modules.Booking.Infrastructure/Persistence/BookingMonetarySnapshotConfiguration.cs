using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Booking.Domain;

namespace TravelCore.Modules.Booking.Infrastructure.Persistence;

internal sealed class BookingMonetarySnapshotConfiguration : IEntityTypeConfiguration<BookingMonetarySnapshot>
{
    public void Configure(EntityTypeBuilder<BookingMonetarySnapshot> builder)
    {
        builder.ToTable("booking_monetary_snapshots", table =>
        {
            table.HasCheckConstraint(
                "ck_booking_monetary_snapshots_quote_expires_after_quoted",
                "quote_expires_at > quoted_at");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => BookingMonetarySnapshotId.From(value));

        builder.Property(x => x.BookingId)
            .HasColumnName("booking_id")
            .HasConversion(id => id.Value, value => BookingId.From(value))
            .IsRequired();

        builder.Property(x => x.QuoteReference)
            .HasColumnName("quote_reference_id")
            .HasConversion(
                reference => reference.LogicalId,
                value => PricingQuoteReference.From(value))
            .IsRequired();

        builder.Property(x => x.SourcePriceId)
            .HasColumnName("source_price_id")
            .IsRequired();

        builder.Property(x => x.TargetType)
            .HasColumnName("snapshot_target_type")
            .HasMaxLength(64);

        builder.Property(x => x.TargetId)
            .HasColumnName("snapshot_target_id");

        builder.Property(x => x.QuotedAt)
            .HasColumnName("quoted_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.QuoteExpiresAt)
            .HasColumnName("quote_expires_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.AcceptedAt)
            .HasColumnName("accepted_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.OwnsRequiredMoney(x => x.Total, "total_amount", "currency_code");

        builder.HasMany(x => x.Components)
            .WithOne()
            .HasForeignKey("SnapshotId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Components)
            .HasField("_components")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.BookingId)
            .IsUnique()
            .HasDatabaseName("ux_booking_monetary_snapshots_booking_id");
    }
}

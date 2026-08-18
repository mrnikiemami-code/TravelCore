using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Booking.Domain;

namespace TravelCore.Modules.Booking.Infrastructure.Persistence;

internal sealed class BookingMonetaryComponentConfiguration : IEntityTypeConfiguration<BookingMonetaryComponent>
{
    public void Configure(EntityTypeBuilder<BookingMonetaryComponent> builder)
    {
        builder.ToTable("booking_monetary_snapshot_components");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => BookingMonetaryComponentId.From(value));

        builder.Property<BookingMonetarySnapshotId>("SnapshotId")
            .HasColumnName("snapshot_id")
            .HasConversion(id => id.Value, value => BookingMonetarySnapshotId.From(value));

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
            .HasMaxLength(BookingMonetaryComponent.CodeMaxLength);

        builder.Property(x => x.Label)
            .HasColumnName("label")
            .HasMaxLength(BookingMonetaryComponent.LabelMaxLength);

        builder.HasIndex("SnapshotId")
            .HasDatabaseName("ix_booking_monetary_snapshot_components_snapshot_id");

        builder.HasIndex("SnapshotId", nameof(BookingMonetaryComponent.SortOrder))
            .IsUnique()
            .HasDatabaseName("ux_booking_monetary_snapshot_components_snapshot_sort");
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelAvailabilityHoldConfiguration : IEntityTypeConfiguration<HotelAvailabilityHold>
{
    public void Configure(EntityTypeBuilder<HotelAvailabilityHold> builder)
    {
        builder.ToTable("hotel_availability_holds", table =>
        {
            table.HasCheckConstraint(
                "ck_hotel_availability_holds_status",
                "status IN (1, 2, 3, 4)");
            table.HasCheckConstraint(
                "ck_hotel_availability_holds_active_expiry",
                "(status <> 2) OR (expires_at IS NOT NULL)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => HotelAvailabilityHoldId.From(value));

        builder.Property(x => x.HotelBookingId)
            .HasColumnName("hotel_booking_id")
            .HasConversion(id => id.Value, value => HotelBookingId.From(value))
            .IsRequired();

        builder.Property(x => x.SourceKey)
            .HasColumnName("source_key")
            .HasMaxLength(HotelAvailabilityHold.SourceKeyMaxLength)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.SourceHoldReference)
            .HasColumnName("source_hold_reference")
            .HasMaxLength(HotelAvailabilityHold.SourceHoldReferenceMaxLength);

        builder.Property(x => x.RequestedAt).HasColumnName("requested_at").IsRequired();
        builder.Property(x => x.ActivatedAt).HasColumnName("activated_at");
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at");
        builder.Property(x => x.ReleasedAt).HasColumnName("released_at");
        builder.Property(x => x.ExpiredAt).HasColumnName("expired_at");
        builder.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();

        builder.Ignore(x => x.IsUnresolved);
        builder.Ignore(x => x.IsTerminal);

        builder.HasOne<Domain.HotelBooking>()
            .WithMany()
            .HasForeignKey(x => x.HotelBookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Rooms)
            .WithOne()
            .HasForeignKey(x => x.HoldId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Rooms)
            .HasField("_rooms")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.HotelBookingId)
            .HasFilter("status IN (1, 2)")
            .IsUnique()
            .HasDatabaseName("ux_hotel_availability_holds_one_unresolved");

        builder.HasIndex(x => new { x.SourceKey, x.SourceHoldReference })
            .HasFilter("source_hold_reference IS NOT NULL")
            .IsUnique()
            .HasDatabaseName("ux_hotel_availability_holds_source_ref");
    }
}

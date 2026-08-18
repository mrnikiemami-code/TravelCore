using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelRateOfferSnapshotConfiguration : IEntityTypeConfiguration<HotelRateOfferSnapshot>
{
    public void Configure(EntityTypeBuilder<HotelRateOfferSnapshot> builder)
    {
        builder.ToTable("hotel_rate_offer_snapshots");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => HotelRateOfferSnapshotId.From(value));

        builder.Property(x => x.HotelBookingId)
            .HasColumnName("hotel_booking_id")
            .HasConversion(id => id.Value, value => HotelBookingId.From(value))
            .IsRequired();

        builder.Property(x => x.Place)
            .HasColumnName("place_id")
            .HasConversion(
                reference => reference.PlaceId,
                value => new HotelPlaceReference(value))
            .IsRequired();

        builder.Property(x => x.CheckInDate).HasColumnName("check_in_date").IsRequired();
        builder.Property(x => x.CheckOutDate).HasColumnName("check_out_date").IsRequired();

        builder.Property(x => x.SourceKey)
            .HasColumnName("source_key")
            .HasMaxLength(HotelRateOfferSnapshot.SourceKeyMaxLength)
            .IsRequired();

        builder.Property(x => x.SourceOfferReference)
            .HasColumnName("source_offer_reference")
            .HasMaxLength(HotelRateOfferSnapshot.SourceOfferReferenceMaxLength)
            .IsRequired();

        builder.Property(x => x.QuotedAt).HasColumnName("quoted_at").IsRequired();
        builder.Property(x => x.OfferExpiresAt).HasColumnName("offer_expires_at");
        builder.Property(x => x.AcceptedAt).HasColumnName("accepted_at").IsRequired();

        builder.HasOne<Domain.HotelBooking>()
            .WithMany()
            .HasForeignKey(x => x.HotelBookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Rooms)
            .WithOne()
            .HasForeignKey(x => x.HotelRateOfferSnapshotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Rooms)
            .HasField("_rooms")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(x => x.Monetary)
            .WithOne()
            .HasForeignKey<HotelBookingMonetarySnapshot>(x => x.HotelRateOfferSnapshotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Monetary).IsRequired();

        builder.HasOne(x => x.CancellationPolicy)
            .WithOne()
            .HasForeignKey<HotelCancellationPolicySnapshot>(x => x.HotelRateOfferSnapshotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.CancellationPolicy).IsRequired();

        builder.HasIndex(x => x.HotelBookingId)
            .IsUnique()
            .HasDatabaseName("ux_hotel_rate_offer_snapshots_hotel_booking_id");
    }
}

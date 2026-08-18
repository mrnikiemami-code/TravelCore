using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelBookingPublicIdempotencyRecordConfiguration
    : IEntityTypeConfiguration<HotelBookingPublicIdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<HotelBookingPublicIdempotencyRecord> builder)
    {
        builder.ToTable("hotel_booking_public_idempotency");
        builder.HasKey(x => x.IdempotencyKey);

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(HotelBookingPublicIdempotencyRecord.KeyMaxLength)
            .IsRequired();

        builder.Property(x => x.HotelBookingId)
            .HasColumnName("hotel_booking_id")
            .HasConversion(id => id.Value, value => HotelBookingId.From(value))
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(x => x.HotelBookingId)
            .HasDatabaseName("ix_hotel_booking_public_idempotency_hotel_booking_id");

        builder.HasOne<Domain.HotelBooking>()
            .WithMany()
            .HasForeignKey(x => x.HotelBookingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

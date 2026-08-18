using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Booking.Domain;

namespace TravelCore.Modules.Booking.Infrastructure.Persistence;

internal sealed class BookingPublicIdempotencyRecordConfiguration
    : IEntityTypeConfiguration<BookingPublicIdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<BookingPublicIdempotencyRecord> builder)
    {
        builder.ToTable("booking_public_idempotency");
        builder.HasKey(x => x.IdempotencyKey);

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(BookingPublicIdempotencyRecord.KeyMaxLength)
            .IsRequired();

        builder.Property(x => x.BookingId)
            .HasColumnName("booking_id")
            .HasConversion(id => id.Value, value => BookingId.From(value))
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(x => x.BookingId)
            .HasDatabaseName("ix_booking_public_idempotency_booking_id");

        builder.HasOne<Domain.Booking>()
            .WithMany()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelBookingOutboxMessageConfiguration
    : IEntityTypeConfiguration<HotelBookingOutboxMessage>
{
    public void Configure(EntityTypeBuilder<HotelBookingOutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.OccurredAt).HasColumnName("occurred_at").IsRequired();
        builder.Property(x => x.MessageType).HasColumnName("message_type").HasMaxLength(256).IsRequired();
        builder.Property(x => x.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.ProcessedAt).HasColumnName("processed_at");
        builder.HasIndex(x => x.ProcessedAt).HasDatabaseName("ix_hotel_booking_outbox_messages_processed_at");
    }
}

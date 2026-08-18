using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelCore.Modules.Payment.Infrastructure.Persistence;

internal sealed class PaymentHotelBookingCancellationRefundInboxRecordConfiguration
    : IEntityTypeConfiguration<PaymentHotelBookingCancellationRefundInboxRecord>
{
    public void Configure(EntityTypeBuilder<PaymentHotelBookingCancellationRefundInboxRecord> builder)
    {
        builder.ToTable("hotel_booking_cancellation_refund_inbox");
        builder.HasKey(x => x.HotelBookingCancellationId);
        builder.Property(x => x.HotelBookingCancellationId).HasColumnName("hotel_booking_cancellation_id");
        builder.Property(x => x.PaymentId).HasColumnName("payment_id").IsRequired();
        builder.Property(x => x.ProcessedAt).HasColumnName("processed_at").IsRequired();
        builder.HasIndex(x => x.PaymentId)
            .IsUnique()
            .HasDatabaseName("ux_hotel_booking_cancellation_refund_inbox_payment_id");
    }
}

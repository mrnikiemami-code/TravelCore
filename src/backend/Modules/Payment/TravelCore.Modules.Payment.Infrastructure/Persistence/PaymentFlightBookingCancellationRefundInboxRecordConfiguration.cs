using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelCore.Modules.Payment.Infrastructure.Persistence;

internal sealed class PaymentFlightBookingCancellationRefundInboxRecordConfiguration
    : IEntityTypeConfiguration<PaymentFlightBookingCancellationRefundInboxRecord>
{
    public void Configure(EntityTypeBuilder<PaymentFlightBookingCancellationRefundInboxRecord> builder)
    {
        builder.ToTable("flight_booking_cancellation_refund_inbox");
        builder.HasKey(x => x.FlightBookingCancellationId);
        builder.Property(x => x.FlightBookingCancellationId).HasColumnName("flight_booking_cancellation_id");
        builder.Property(x => x.PaymentId).HasColumnName("payment_id").IsRequired();
        builder.Property(x => x.ProcessedAt).HasColumnName("processed_at").IsRequired();
        builder.HasIndex(x => x.PaymentId)
            .IsUnique()
            .HasDatabaseName("ux_flight_booking_cancellation_refund_inbox_payment_id");
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelBookingPaymentSuccessInboxRecordConfiguration
    : IEntityTypeConfiguration<HotelBookingPaymentSuccessInboxRecord>
{
    public void Configure(EntityTypeBuilder<HotelBookingPaymentSuccessInboxRecord> builder)
    {
        builder.ToTable("payment_success_inbox");
        builder.HasKey(x => x.PaymentId);
        builder.Property(x => x.PaymentId).HasColumnName("payment_id");
        builder.Property(x => x.ProcessedAt).HasColumnName("processed_at").IsRequired();
    }
}

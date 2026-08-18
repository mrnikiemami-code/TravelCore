using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelBookingRefundSuccessInboxRecordConfiguration
    : IEntityTypeConfiguration<HotelBookingRefundSuccessInboxRecord>
{
    public void Configure(EntityTypeBuilder<HotelBookingRefundSuccessInboxRecord> builder)
    {
        builder.ToTable("refund_success_inbox");
        builder.HasKey(x => x.RefundId);
        builder.Property(x => x.RefundId).HasColumnName("refund_id");
        builder.Property(x => x.ProcessedAt).HasColumnName("processed_at").IsRequired();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelCore.Modules.Booking.Infrastructure.Persistence;

internal sealed class RefundSuccessInboxRecordConfiguration
    : IEntityTypeConfiguration<RefundSuccessInboxRecord>
{
    public void Configure(EntityTypeBuilder<RefundSuccessInboxRecord> builder)
    {
        builder.ToTable("refund_success_inbox");
        builder.HasKey(x => x.RefundId);
        builder.Property(x => x.RefundId).HasColumnName("refund_id");
        builder.Property(x => x.ProcessedAt).HasColumnName("processed_at").IsRequired();
    }
}

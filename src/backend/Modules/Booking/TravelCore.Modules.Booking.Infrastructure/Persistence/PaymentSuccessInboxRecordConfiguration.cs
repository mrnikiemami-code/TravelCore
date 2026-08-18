using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelCore.Modules.Booking.Infrastructure.Persistence;

internal sealed class PaymentSuccessInboxRecordConfiguration
    : IEntityTypeConfiguration<PaymentSuccessInboxRecord>
{
    public void Configure(EntityTypeBuilder<PaymentSuccessInboxRecord> builder)
    {
        builder.ToTable("payment_success_inbox");
        builder.HasKey(x => x.PaymentId);
        builder.Property(x => x.PaymentId).HasColumnName("payment_id");
        builder.Property(x => x.ProcessedAt).HasColumnName("processed_at").IsRequired();
    }
}

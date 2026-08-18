using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelCore.Modules.Payment.Infrastructure.Persistence;

internal sealed class PaymentCompensationInboxRecordConfiguration
    : IEntityTypeConfiguration<PaymentCompensationInboxRecord>
{
    public void Configure(EntityTypeBuilder<PaymentCompensationInboxRecord> builder)
    {
        builder.ToTable("compensation_inbox");
        builder.HasKey(x => x.PaymentId);
        builder.Property(x => x.PaymentId).HasColumnName("payment_id");
        builder.Property(x => x.ProcessedAt).HasColumnName("processed_at").IsRequired();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightRefundSuccessInboxRecordConfiguration
    : IEntityTypeConfiguration<FlightRefundSuccessInboxRecord>
{
    public void Configure(EntityTypeBuilder<FlightRefundSuccessInboxRecord> builder)
    {
        builder.ToTable("flight_refund_success_inbox");
        builder.HasKey(x => x.RefundId);
        builder.Property(x => x.RefundId).HasColumnName("refund_id");
        builder.Property(x => x.ProcessedAt).HasColumnName("processed_at").IsRequired();
    }
}

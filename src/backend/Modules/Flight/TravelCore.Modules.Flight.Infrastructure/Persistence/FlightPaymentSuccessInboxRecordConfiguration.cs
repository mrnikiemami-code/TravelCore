using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightPaymentSuccessInboxRecordConfiguration
    : IEntityTypeConfiguration<FlightPaymentSuccessInboxRecord>
{
    public void Configure(EntityTypeBuilder<FlightPaymentSuccessInboxRecord> builder)
    {
        builder.ToTable("flight_payment_success_inbox");
        builder.HasKey(x => x.PaymentId);
        builder.Property(x => x.PaymentId).HasColumnName("payment_id");
        builder.Property(x => x.ProcessedAt).HasColumnName("processed_at").IsRequired();
    }
}

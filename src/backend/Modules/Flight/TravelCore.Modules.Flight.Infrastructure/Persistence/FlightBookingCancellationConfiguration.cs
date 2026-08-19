using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightBookingCancellationConfiguration : IEntityTypeConfiguration<FlightBookingCancellation>
{
    public void Configure(EntityTypeBuilder<FlightBookingCancellation> builder)
    {
        builder.ToTable("flight_booking_cancellations", table =>
        {
            table.HasCheckConstraint(
                "ck_flight_booking_cancellations_status",
                "status IN (1, 2, 3, 4)");
            table.HasCheckConstraint(
                "ck_flight_booking_cancellations_financial_outcome",
                "financial_outcome IN (1, 2)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => FlightBookingCancellationId.From(value));

        builder.Property(x => x.FlightBookingId)
            .HasColumnName("flight_booking_id")
            .HasConversion(id => id.Value, value => FlightBookingId.From(value))
            .IsRequired();

        builder.Property(x => x.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired();

        builder.Property(x => x.RequestedAt).HasColumnName("requested_at").IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.FinancialOutcome)
            .HasColumnName("financial_outcome")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.PenaltyAmount)
            .HasColumnName("penalty_amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.RefundAmount)
            .HasColumnName("refund_amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CurrencyCode)
            .HasColumnName("currency_code")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.CompletedAt).HasColumnName("completed_at");
        builder.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();

        builder.Ignore(x => x.HasUnresolvedAttempt);
        builder.Ignore(x => x.RequiresFullRefund);

        builder.HasOne<Domain.FlightBooking>()
            .WithMany()
            .HasForeignKey(x => x.FlightBookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Attempts)
            .WithOne()
            .HasForeignKey(x => x.CancellationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Attempts)
            .HasField("_attempts")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.FlightBookingId)
            .IsUnique()
            .HasDatabaseName("ux_flight_booking_cancellations_flight_booking_id");
    }
}

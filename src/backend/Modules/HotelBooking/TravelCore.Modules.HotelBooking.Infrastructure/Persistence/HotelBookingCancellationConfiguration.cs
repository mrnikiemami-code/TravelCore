using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelBookingCancellationConfiguration : IEntityTypeConfiguration<HotelBookingCancellation>
{
    public void Configure(EntityTypeBuilder<HotelBookingCancellation> builder)
    {
        builder.ToTable("hotel_booking_cancellations", table =>
        {
            table.HasCheckConstraint(
                "ck_hotel_booking_cancellations_status",
                "status IN (1, 2, 3, 4)");
            table.HasCheckConstraint(
                "ck_hotel_booking_cancellations_financial_outcome",
                "financial_outcome IN (1, 2)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => HotelBookingCancellationId.From(value));

        builder.Property(x => x.HotelBookingId)
            .HasColumnName("hotel_booking_id")
            .HasConversion(id => id.Value, value => HotelBookingId.From(value))
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

        builder.HasOne<Domain.HotelBooking>()
            .WithMany()
            .HasForeignKey(x => x.HotelBookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Attempts)
            .WithOne()
            .HasForeignKey(x => x.CancellationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Attempts)
            .HasField("_attempts")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.HotelBookingId)
            .IsUnique()
            .HasDatabaseName("ux_hotel_booking_cancellations_hotel_booking_id");
    }
}

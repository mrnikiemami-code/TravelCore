using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Money;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;

namespace TravelCore.Modules.Payment.Infrastructure.Persistence;

internal sealed class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("refunds", table =>
        {
            table.HasCheckConstraint("ck_refunds_status", "status IN (1, 2)");
            table.HasCheckConstraint("ck_refunds_version_nonnegative", "version >= 0");
            table.HasCheckConstraint(
                "ck_refunds_exactly_one_target",
                "(booking_id IS NOT NULL AND hotel_booking_id IS NULL) OR (booking_id IS NULL AND hotel_booking_id IS NOT NULL)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => RefundId.From(value));

        builder.Property(x => x.PaymentId)
            .HasColumnName("payment_id")
            .HasConversion(id => id.Value, value => PaymentId.From(value))
            .IsRequired();

        builder.Property(x => x.Booking)
            .HasColumnName("booking_id")
            .HasConversion(
                reference => reference.HasValue ? reference.Value.BookingId : (Guid?)null,
                value => value.HasValue ? new BookingReference(value.Value) : null);

        builder.Property(x => x.HotelBooking)
            .HasColumnName("hotel_booking_id")
            .HasConversion(
                reference => reference.HasValue ? reference.Value.HotelBookingId : (Guid?)null,
                value => value.HasValue ? new HotelBookingPaymentReference(value.Value) : null);

        builder.Ignore(x => x.TargetKind);
        builder.Ignore(x => x.TargetReferenceId);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.StatusChangedAt)
            .HasColumnName("status_changed_at")
            .IsRequired();

        builder.Property(x => x.SucceededAt)
            .HasColumnName("succeeded_at");

        builder.Property(x => x.Version)
            .HasColumnName("version")
            .IsConcurrencyToken()
            .IsRequired();

        builder.OwnsOne(x => x.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("amount")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasConversion(c => c.Value, value => CurrencyCode.Parse(value))
                .HasColumnName("currency")
                .HasMaxLength(3);
        });

        builder.HasOne<PaymentAggregate>()
            .WithMany()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Attempts)
            .WithOne()
            .HasForeignKey("RefundId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Attempts)
            .HasField("_attempts")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.PaymentId)
            .IsUnique()
            .HasDatabaseName("ux_refunds_payment_id");
    }
}

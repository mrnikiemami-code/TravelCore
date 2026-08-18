using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Money;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;

namespace TravelCore.Modules.Payment.Infrastructure.Persistence;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<PaymentAggregate>
{
    public void Configure(EntityTypeBuilder<PaymentAggregate> builder)
    {
        builder.ToTable("payments", table =>
        {
            table.HasCheckConstraint("ck_payments_status", "status IN (1, 2)");
            table.HasCheckConstraint("ck_payments_version_nonnegative", "version >= 0");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => PaymentId.From(value));

        builder.Property(x => x.Booking)
            .HasColumnName("booking_id")
            .HasConversion(
                reference => reference.BookingId,
                value => new BookingReference(value))
            .IsRequired();

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

        builder.OwnsOne(x => x.ExecutionSnapshot, owned =>
        {
            owned.Property(x => x.BookingSnapshotId)
                .HasColumnName("booking_snapshot_id");
            owned.Property(x => x.CapturedAt)
                .HasColumnName("execution_captured_at");
            owned.OwnsOne(x => x.Amount, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("execution_amount")
                    .HasPrecision(18, 2);
                money.Property(m => m.Currency)
                    .HasConversion(c => c.Value, value => CurrencyCode.Parse(value))
                    .HasColumnName("execution_currency")
                    .HasMaxLength(3);
            });
        });

        builder.HasMany(x => x.Attempts)
            .WithOne()
            .HasForeignKey("PaymentId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Attempts)
            .HasField("_attempts")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.Booking)
            .IsUnique()
            .HasDatabaseName("ux_payments_booking_id");
    }
}

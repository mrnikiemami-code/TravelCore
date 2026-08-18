using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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

        builder.HasMany(x => x.Attempts)
            .WithOne()
            .HasForeignKey("PaymentId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Attempts)
            .HasField("_attempts")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.Booking)
            .HasDatabaseName("ix_payments_booking_id");
    }
}

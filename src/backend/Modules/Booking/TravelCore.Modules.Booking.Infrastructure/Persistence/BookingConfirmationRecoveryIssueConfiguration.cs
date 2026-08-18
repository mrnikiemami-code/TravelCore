using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Booking.Domain;

namespace TravelCore.Modules.Booking.Infrastructure.Persistence;

internal sealed class BookingConfirmationRecoveryIssueConfiguration
    : IEntityTypeConfiguration<BookingConfirmationRecoveryIssue>
{
    public void Configure(EntityTypeBuilder<BookingConfirmationRecoveryIssue> builder)
    {
        builder.ToTable("booking_confirmation_recovery_issues", table =>
        {
            table.HasCheckConstraint(
                "ck_booking_confirmation_recovery_issues_reason",
                "reason IN (1, 2, 3, 4, 5, 6)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.BookingId)
            .HasColumnName("booking_id")
            .HasConversion(id => id.Value, value => BookingId.From(value))
            .IsRequired();

        builder.Property(x => x.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .IsRequired();

        builder.Property(x => x.DetectedAt)
            .HasColumnName("detected_at")
            .IsRequired();

        builder.HasOne<Domain.Booking>()
            .WithMany()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.BookingId)
            .IsUnique()
            .HasDatabaseName("ux_booking_confirmation_recovery_issues_booking_id");
    }
}

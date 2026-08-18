using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelBookingReconciliationIssueConfiguration
    : IEntityTypeConfiguration<HotelBookingReconciliationIssue>
{
    public void Configure(EntityTypeBuilder<HotelBookingReconciliationIssue> builder)
    {
        builder.ToTable("hotel_booking_reconciliation_issues", table =>
        {
            table.HasCheckConstraint(
                "ck_hotel_booking_reconciliation_issues_kind",
                "kind IN (1, 2, 3, 4, 5, 6, 7, 8)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => HotelBookingReconciliationIssueId.From(value));

        builder.Property(x => x.HotelBookingId)
            .HasColumnName("hotel_booking_id")
            .HasConversion(id => id.Value, value => HotelBookingId.From(value))
            .IsRequired();

        builder.Property(x => x.ReservationId)
            .HasColumnName("hotel_supplier_reservation_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? HotelSupplierReservationId.From(value.Value) : null);

        builder.Property(x => x.AttemptId)
            .HasColumnName("hotel_supplier_reservation_attempt_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? HotelSupplierReservationAttemptId.From(value.Value) : null);

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.Property(x => x.Detail)
            .HasColumnName("detail")
            .HasMaxLength(HotelBookingReconciliationIssue.DetailMaxLength)
            .IsRequired();

        builder.Ignore(x => x.BlocksConfirmation);

        builder.HasOne<Domain.HotelBooking>()
            .WithMany()
            .HasForeignKey(x => x.HotelBookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.HotelBookingId)
            .HasDatabaseName("ix_hotel_booking_reconciliation_issues_hotel_booking_id");
    }
}

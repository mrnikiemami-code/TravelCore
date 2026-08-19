using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightReconciliationIssueConfiguration : IEntityTypeConfiguration<FlightReconciliationIssue>
{
    public void Configure(EntityTypeBuilder<FlightReconciliationIssue> builder)
    {
        builder.ToTable("flight_reconciliation_issues", table =>
        {
            table.HasCheckConstraint(
                "ck_flight_reconciliation_issues_kind",
                "kind IN (1, 2, 3, 4, 5, 6, 7, 8, 9)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => FlightReconciliationIssueId.From(value));

        builder.Property(x => x.FlightBookingId)
            .HasColumnName("flight_booking_id")
            .HasConversion(id => id.Value, value => FlightBookingId.From(value))
            .IsRequired();

        builder.Property(x => x.ReservationId)
            .HasColumnName("flight_supplier_reservation_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? FlightSupplierReservationId.From(value.Value) : null);

        builder.Property(x => x.AttemptId)
            .HasColumnName("flight_supplier_reservation_attempt_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? FlightSupplierReservationAttemptId.From(value.Value) : null);

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.Property(x => x.Detail)
            .HasColumnName("detail")
            .HasMaxLength(FlightReconciliationIssue.DetailMaxLength)
            .IsRequired();

        builder.Ignore(x => x.BlocksConfirmation);

        builder.HasOne<Domain.FlightBooking>()
            .WithMany()
            .HasForeignKey(x => x.FlightBookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.FlightBookingId)
            .HasDatabaseName("ix_flight_reconciliation_issues_flight_booking_id");
    }
}

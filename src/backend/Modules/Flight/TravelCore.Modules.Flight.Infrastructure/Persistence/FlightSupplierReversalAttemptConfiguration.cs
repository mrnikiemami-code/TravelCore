using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightSupplierReversalAttemptConfiguration
    : IEntityTypeConfiguration<FlightSupplierReversalAttempt>
{
    public void Configure(EntityTypeBuilder<FlightSupplierReversalAttempt> builder)
    {
        builder.ToTable("flight_supplier_reversal_attempts", table =>
        {
            table.HasCheckConstraint(
                "ck_flight_supplier_reversal_attempts_status",
                "status IN (1, 2, 3, 4)");
            table.HasCheckConstraint(
                "ck_flight_supplier_reversal_attempts_kind",
                "kind IN (1, 2, 3)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => FlightSupplierReversalAttemptId.From(value));

        builder.Property(x => x.CancellationId)
            .HasColumnName("flight_booking_cancellation_id")
            .HasConversion(id => id.Value, value => FlightBookingCancellationId.From(value))
            .IsRequired();

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.TicketId)
            .HasColumnName("ticket_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? FlightTicketId.From(value.Value) : null);

        builder.Property(x => x.PassengerId)
            .HasColumnName("passenger_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? FlightPassengerId.From(value.Value) : null);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.InitiatedAt).HasColumnName("initiated_at");
        builder.Property(x => x.SucceededAt).HasColumnName("succeeded_at");
        builder.Property(x => x.FailedAt).HasColumnName("failed_at");

        builder.Ignore(x => x.IsUnresolved);
        builder.Ignore(x => x.IsTerminal);

        builder.HasIndex(x => new { x.CancellationId, x.Kind, x.TicketId })
            .HasFilter("status IN (1, 2) AND ticket_id IS NOT NULL")
            .IsUnique()
            .HasDatabaseName("ux_flight_supplier_reversal_attempts_one_unresolved_ticket");

        builder.HasIndex(x => new { x.CancellationId, x.Kind })
            .HasFilter("status IN (1, 2) AND ticket_id IS NULL")
            .IsUnique()
            .HasDatabaseName("ux_flight_supplier_reversal_attempts_one_unresolved_reservation");
    }
}

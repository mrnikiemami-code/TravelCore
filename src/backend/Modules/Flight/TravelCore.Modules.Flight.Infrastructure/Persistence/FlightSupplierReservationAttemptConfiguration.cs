using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightSupplierReservationAttemptConfiguration
    : IEntityTypeConfiguration<FlightSupplierReservationAttempt>
{
    public void Configure(EntityTypeBuilder<FlightSupplierReservationAttempt> builder)
    {
        builder.ToTable("flight_supplier_reservation_attempts", table =>
        {
            table.HasCheckConstraint(
                "ck_flight_supplier_reservation_attempts_status",
                "status IN (1, 2, 3, 4)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => FlightSupplierReservationAttemptId.From(value));

        builder.Property(x => x.ReservationId)
            .HasColumnName("flight_supplier_reservation_id")
            .HasConversion(id => id.Value, value => FlightSupplierReservationId.From(value))
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.InitiatedAt).HasColumnName("initiated_at");
        builder.Property(x => x.ConfirmedAt).HasColumnName("confirmed_at");
        builder.Property(x => x.FailedAt).HasColumnName("failed_at");

        builder.Ignore(x => x.IsUnresolved);
        builder.Ignore(x => x.IsTerminal);

        builder.HasIndex(x => x.ReservationId)
            .HasFilter("status IN (1, 2)")
            .IsUnique()
            .HasDatabaseName("ux_flight_supplier_reservation_attempts_one_unresolved");
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightSupplierReservationConfiguration : IEntityTypeConfiguration<FlightSupplierReservation>
{
    public void Configure(EntityTypeBuilder<FlightSupplierReservation> builder)
    {
        builder.ToTable("flight_supplier_reservations", table =>
        {
            table.HasCheckConstraint(
                "ck_flight_supplier_reservations_status",
                "status IN (1, 2, 3, 4)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => FlightSupplierReservationId.From(value));

        builder.Property(x => x.FlightBookingId)
            .HasColumnName("flight_booking_id")
            .HasConversion(id => id.Value, value => FlightBookingId.From(value))
            .IsRequired();

        builder.Property(x => x.SourceKey)
            .HasColumnName("source_key")
            .HasMaxLength(FlightSupplierReservation.SourceKeyMaxLength)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.SourceReservationReference)
            .HasColumnName("source_reservation_reference")
            .HasMaxLength(FlightSupplierReservation.SourceReservationReferenceMaxLength);

        builder.Property(x => x.ReservationLocator)
            .HasColumnName("reservation_locator")
            .HasMaxLength(FlightSupplierReservation.ReservationLocatorMaxLength);

        builder.Property(x => x.ReservationExpiresAt).HasColumnName("reservation_expires_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.ConfirmedAt).HasColumnName("confirmed_at");
        builder.Property(x => x.ExpiredAt).HasColumnName("expired_at");
        builder.Property(x => x.CancelledAt).HasColumnName("cancelled_at");
        builder.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();

        builder.Ignore(x => x.HasUnresolvedAttempt);

        builder.HasOne<Domain.FlightBooking>()
            .WithMany()
            .HasForeignKey(x => x.FlightBookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Attempts)
            .WithOne()
            .HasForeignKey(x => x.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Attempts)
            .HasField("_attempts")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.FlightBookingId)
            .IsUnique()
            .HasDatabaseName("ux_flight_supplier_reservations_flight_booking_id");

        builder.HasIndex(x => new { x.SourceKey, x.SourceReservationReference })
            .HasFilter("source_reservation_reference IS NOT NULL")
            .IsUnique()
            .HasDatabaseName("ux_flight_supplier_reservations_source_ref");
    }
}

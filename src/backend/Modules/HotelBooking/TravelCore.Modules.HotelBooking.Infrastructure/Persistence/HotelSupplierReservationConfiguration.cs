using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelSupplierReservationConfiguration : IEntityTypeConfiguration<HotelSupplierReservation>
{
    public void Configure(EntityTypeBuilder<HotelSupplierReservation> builder)
    {
        builder.ToTable("hotel_supplier_reservations", table =>
        {
            table.HasCheckConstraint(
                "ck_hotel_supplier_reservations_status",
                "status IN (1, 2, 3)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => HotelSupplierReservationId.From(value));

        builder.Property(x => x.HotelBookingId)
            .HasColumnName("hotel_booking_id")
            .HasConversion(id => id.Value, value => HotelBookingId.From(value))
            .IsRequired();

        builder.Property(x => x.SourceKey)
            .HasColumnName("source_key")
            .HasMaxLength(HotelSupplierReservation.SourceKeyMaxLength)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.SourceReservationReference)
            .HasColumnName("source_reservation_reference")
            .HasMaxLength(HotelSupplierReservation.SourceReservationReferenceMaxLength);

        builder.Property(x => x.SupplierConfirmationCode)
            .HasColumnName("supplier_confirmation_code")
            .HasMaxLength(HotelSupplierReservation.ConfirmationCodeMaxLength);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.ConfirmedAt).HasColumnName("confirmed_at");
        builder.Property(x => x.CancelledAt).HasColumnName("cancelled_at");
        builder.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();

        builder.Ignore(x => x.HasUnresolvedAttempt);

        builder.HasOne<Domain.HotelBooking>()
            .WithMany()
            .HasForeignKey(x => x.HotelBookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Attempts)
            .WithOne()
            .HasForeignKey(x => x.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Attempts)
            .HasField("_attempts")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.HotelBookingId)
            .IsUnique()
            .HasDatabaseName("ux_hotel_supplier_reservations_hotel_booking_id");

        builder.HasIndex(x => new { x.SourceKey, x.SourceReservationReference })
            .HasFilter("source_reservation_reference IS NOT NULL")
            .IsUnique()
            .HasDatabaseName("ux_hotel_supplier_reservations_source_ref");
    }
}

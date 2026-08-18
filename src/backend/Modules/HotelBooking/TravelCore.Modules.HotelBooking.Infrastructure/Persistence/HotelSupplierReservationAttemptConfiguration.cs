using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelSupplierReservationAttemptConfiguration
    : IEntityTypeConfiguration<HotelSupplierReservationAttempt>
{
    public void Configure(EntityTypeBuilder<HotelSupplierReservationAttempt> builder)
    {
        builder.ToTable("hotel_supplier_reservation_attempts", table =>
        {
            table.HasCheckConstraint(
                "ck_hotel_supplier_reservation_attempts_status",
                "status IN (1, 2, 3, 4)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => HotelSupplierReservationAttemptId.From(value));

        builder.Property(x => x.ReservationId)
            .HasColumnName("hotel_supplier_reservation_id")
            .HasConversion(id => id.Value, value => HotelSupplierReservationId.From(value))
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
            .HasDatabaseName("ux_hotel_supplier_reservation_attempts_one_unresolved");
    }
}
